using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace AutoJumper
{
	public partial class AutoJumper : Form
	{
		private bool mIfDebug=false;
		private Bitmap mBmpDebug;

		private const string msAdbPath = "adb.exe";
		private const string msScreenPath = @"C:\Downloads\Screen.png";//1280*720
		private bool mIsJumping = false;

		private int mnJumpCntr=0;//Jump次数，无实际用处
		private Image mImgScreen;//截屏图片
		private Image mImgScrnHist1;//历史截屏图片，保留失败那一次记录
		private Image mImgScrnHist2;//历史截屏图片，保留失败那一次记录
		private Rectangle mRctEffect;//有效区域，(0, 300, 720, 600)
		private Point mPntJmpr;//Jumper位置
		private Rectangle mRctJmpr;//Jumper区域
		private Point mPntDest;//目标位置
		private Rectangle mRctDest;//目标区域
		private Point mPntCenter= new Point(375, 355);//对称中心

		public AutoJumper()
		{
			InitializeComponent();
		}
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string sSection, string sKey, string sValue, string sFilePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string sSection, string sKey, string sDefault, StringBuilder sBldrValue, int nSize, string sFilePath);
		private void JumpHelper_Load(object sender, EventArgs e)
		{
			StringBuilder sIniValue=new StringBuilder(255);
			GetPrivateProfileString("MainParam", "StepRatio", "2.05", sIniValue, 255, ".\\Settings.ini");
			numStepRatio.Value=(decimal)double.Parse(sIniValue.ToString());
			GetPrivateProfileString("MainParam", "Wait", "3.0", sIniValue, 255, ".\\Settings.ini");
			numWait.Value=(decimal)double.Parse(sIniValue.ToString());
		}
		private void JumpHelper_FormClosed(object sender, FormClosedEventArgs e)
		{
			WritePrivateProfileString("MainParam", "StepRatio", numStepRatio.Value.ToString(), ".\\Settings.ini");
			WritePrivateProfileString("MainParam", "Wait", numWait.Value.ToString(), ".\\Settings.ini");
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (!mIsJumping)//启动
			{
				mIsJumping = true;
				btnStart.Text = "停止";
				mnJumpCntr = 0;
				Routine();
			}
			else//停止
			{
				mIsJumping = false;
				btnStart.Text = "启动";
				timer1.Enabled = false;
			}
		}
		private bool sendTouchCmd(int duration)
		{
			bool result = false;
			try
			{
				using (Process myPro = new Process())
				{
					myPro.StartInfo.FileName = "cmd.exe";
					myPro.StartInfo.UseShellExecute = false;
					myPro.StartInfo.RedirectStandardInput = true;
					myPro.StartInfo.RedirectStandardOutput = true;
					myPro.StartInfo.RedirectStandardError = true;
					myPro.StartInfo.CreateNoWindow = true;
					myPro.Start();					
					string str = string.Format(@"""{0}"" {1} {2} {3}", msAdbPath, "shell input swipe 300 800 300 800", duration, "&exit");

					myPro.StandardInput.WriteLine(str);
					myPro.StandardInput.AutoFlush = true;
					// myPro.WaitForExit();

					result = true;
				}
			}
			catch
			{

			}
			return result;
		}
		private void ScreenCapCmd(string sPath)
		{
			using (Process myPro = new Process())
			{
				myPro.StartInfo.FileName = "cmd.exe";
				myPro.StartInfo.UseShellExecute = false;
				myPro.StartInfo.RedirectStandardInput = true;
				myPro.StartInfo.RedirectStandardOutput = true;
				myPro.StartInfo.RedirectStandardError = true;
				myPro.StartInfo.CreateNoWindow = true;

				myPro.Start();
				string str = string.Format(@"""{0}"" shell screencap -p > {1} &exit", msAdbPath, sPath);
				myPro.StandardInput.WriteLine(str);
				myPro.StandardInput.AutoFlush = true;
				myPro.WaitForExit();
			}
		}
		private void btnTestADB_Click(object sender, EventArgs e)
		{
			try
			{
				using (Process myPro = new Process())
				{
					myPro.StartInfo.FileName = "cmd.exe";
					myPro.StartInfo.UseShellExecute = false;
					myPro.StartInfo.RedirectStandardInput = true;
					myPro.StartInfo.RedirectStandardOutput = true;
					myPro.StartInfo.RedirectStandardError = true;
					myPro.StartInfo.CreateNoWindow = true;
					myPro.Start();
					string str = string.Format(@"""{0}"" {1} {2}", msAdbPath, "devices", "&exit");

					myPro.StandardInput.WriteLine(str);
					myPro.StandardInput.AutoFlush = true;					
					myPro.WaitForExit();
					string output = myPro.StandardOutput.ReadToEnd();
					if (!output.Contains("List of devices attached"))
					{
						MessageBox.Show("ADB.exe 文件不存在！");
						return;
					}
					output = output.Substring(output.LastIndexOf("List of devices attached") + 24);
					if (output.Contains("device"))
					{
						MessageBox.Show("连接成功！\n" + output);
						picBoard.Image = mImgScreen = GetScreenCap(msScreenPath);
					}
					else if (output.Contains("unauthorized"))
					{
						MessageBox.Show("请在手机上点击“接受”，以启动USB调试模式。\n" + output);
					}
					else
					{
						MessageBox.Show("连接失败！\n" + output);
					}
				}
			}
			catch
			{

			}
		}
		public Image GetScreenCap(string sPath)
		{
			try
			{
				ScreenCapCmd(sPath);
				FileStream fs = new FileStream(sPath, FileMode.Open, FileAccess.Read);
				if (fs.Length < 1000)//文件有效
				{
					fs.Close();
					txtInfo.Text = "获取截屏失败！";
					return null;
				}

				byte[] zBuf = new byte[fs.Length];
				fs.Read(zBuf, 0, (int)fs.Length);
				MemoryStream stm = new MemoryStream((int)fs.Length);//
				if (zBuf[5] == 0x0a)
				{
					fs.CopyTo(stm);
				}
				else if (zBuf[5] == 0x0d && zBuf[6] == 0x0a)
				{
					for (int i = 0; i < fs.Length - 1; ++i)
					{
						if (zBuf[i] == 0x0d && zBuf[i + 1] == 0x0a)
						{
							stm.WriteByte(0x0a);
							++i;
						}
						else
							stm.WriteByte(zBuf[i]);
					}
					stm.WriteByte(zBuf[fs.Length - 1]);
				}
				else if (zBuf[5] == 0x0d && zBuf[6] == 0x0d && zBuf[7] == 0x0a)
				{
					for (int i = 0; i < fs.Length - 2; ++i)
					{
						if (zBuf[i] == 0x0d && zBuf[i + 1] == 0x0d && zBuf[i + 2] == 0x0a)
						{
							stm.WriteByte(0x0a);
							i += 2;
						}
						else
							stm.WriteByte(zBuf[i]);
					}
					stm.WriteByte(zBuf[fs.Length - 2]);
					stm.WriteByte(zBuf[fs.Length - 1]);
				}
				else
				{
					txtInfo.Text = "获取的PNG格式比较特殊，本程序尚未设计处理方法！";
					stm.Close();
					fs.Close();
					return null;
				}
				stm.Position = 0;
				Image imgCap = Image.FromStream(stm);
				stm.Close();
				fs.Close();
				return imgCap;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.GetType().ToString() + ex.Message);
			}
			return null;
		}
		private void btnGetScreenWin_Click(object sender, EventArgs e)
		{
			if (mImgScreen != null)
				mImgScreen.Save(".\\screen0.bmp");
			if (mImgScrnHist1 != null)
				mImgScrnHist1.Save(".\\screen1.bmp");
			if (mImgScrnHist2 != null)
				mImgScrnHist2.Save(".\\screen2.bmp");
		}
		private bool RoutineFindJumperDest()
		{
			mRctEffect.X = 0;//有效区域
			mRctEffect.Y = 300 * mImgScreen.Height / 1280;
			mRctEffect.Width = mImgScreen.Width;
			mRctEffect.Height = 600 * mImgScreen.Height / 1280;
			mPntCenter.X = 375 * mImgScreen.Width / 720;//对称中心在有效区域的位置
			mPntCenter.Y = 355 * mImgScreen.Height / 1280;

			Bitmap bmpEffect = CutImage(mImgScreen, mRctEffect);//有效区域图像
			if (mIfDebug)
				mBmpDebug = (Bitmap)bmpEffect.Clone();
			float[,,] matHsv = RGB2HSV(bmpEffect);
			mRctJmpr = FindJumper(matHsv);//Jumper区域
			if (mRctJmpr.IsEmpty)
				return false;
			if (((double)mRctJmpr.Height / mRctJmpr.Width < 2.0) || ((double)mRctJmpr.Height / mRctJmpr.Width > 3.0))
				return false;
			mPntJmpr.X = mRctJmpr.X +mRctJmpr.Width / 2;//Jumper在有效区域内的位置
			mPntJmpr.Y = mRctJmpr.Y +mRctJmpr.Height - 10 * mImgScreen.Height / 1280;

			mPntDest.X = mPntCenter.X * 2 - mPntJmpr.X;//Dest与Jumper关于中心点对称（事实上是与Jumper所在的平台）
			mPntDest.Y = mPntCenter.Y * 2 - mPntJmpr.Y;
			mRctDest=FindDest(bmpEffect, mPntDest);//Dest区域
			mPntDest.X = mRctDest.X + mRctDest.Width / 2;//Dest在有效区域内的位置
			mPntDest.Y = mRctDest.Y + mRctDest.Height / 2;
			if ((mPntJmpr.X < mPntCenter.X && mPntDest.X < mPntCenter.X)
				|| (mPntJmpr.X > mPntCenter.X && mPntDest.X > mPntCenter.X)
				|| (mPntJmpr.Y < mPntCenter.Y && mPntDest.Y < mPntCenter.Y)
				|| (mPntJmpr.Y > mPntCenter.Y && mPntDest.Y > mPntCenter.Y))//如果Dest点与Jumper点在中心点同侧，说明FindDest检测错误
			{
				mPntDest.X = mPntCenter.X * 2 - mPntJmpr.X;//则Dest取Jumper的中心对称点
				mPntDest.Y = mPntCenter.Y * 2 - mPntJmpr.Y;
			}

			mRctDest.X += mRctEffect.X;//Dest在全幅图像中的区域
			mRctDest.Y += mRctEffect.Y;
			mPntDest.X += mRctEffect.X;//Dest在全幅图像中的位置
			mPntDest.Y += mRctEffect.Y;

			mRctJmpr.X += mRctEffect.X;//Jumper在全幅图像中的区域
			mRctJmpr.Y += mRctEffect.Y;
			mPntJmpr.X += mRctEffect.X;//Jumper在全幅图像中的位置
			mPntJmpr.Y += mRctEffect.Y;
			return true;
		}
		private void RoutineDrawInfo()
		{
			Graphics painter = Graphics.FromImage(picBoard.Image);
			painter.DrawRectangle(new Pen(Color.Red), mRctJmpr);
			painter.FillPie(new SolidBrush(Color.Yellow), new Rectangle(mPntJmpr.X - 5, mPntJmpr.Y - 5, 10, 10), 0, 360);
			painter.DrawRectangle(new Pen(Color.Green), mRctDest);
			painter.FillPie(new SolidBrush(Color.Blue), new Rectangle(mPntDest.X - 5, mPntDest.Y - 5, 10, 10), 0, 360);
			txtInfo.Text = string.Format("{0}: ({1} ,{2})->({3} ,{4})", mnJumpCntr, mPntJmpr.X, mPntJmpr.Y, mPntDest.X, mPntDest.Y);
		}
		private void Routine()
		{
			picBoard.Image = mImgScreen = GetScreenCap(msScreenPath);
			mnJumpCntr++;
			bool bIfSec=RoutineFindJumperDest();
			RoutineDrawInfo();
			if (!bIfSec)
			{
				if (mImgScrnHist1!=null)
					mImgScrnHist1.Save(".\\screen1.bmp");
				if (mImgScrnHist2!=null)
					mImgScrnHist2.Save(".\\screen2.bmp");
				txtInfo.Text = string.Format("检测Jumper失败！Seg:({0}, {1}, {2}, {3})", mRctJmpr.X, mRctJmpr.Y, mRctJmpr.Width, mRctJmpr.Height);
				mIsJumping = false;
				btnStart.Text = "启动";
				return;//停止
			}
			sendTouchCmd(TouchDuration(mPntJmpr, mPntDest));
			timer1.Interval = (int)(numWait.Value*1000);
			timer1.Enabled = true;
			mImgScrnHist2 = mImgScrnHist1;//记录历史
			mImgScrnHist1 = mImgScreen;//记录历史
		}
		public struct DomainTag//连通域标签
		{
			public int _DomInd;//连通域索引编号，缺省为-1
			public bool _IsTarget;//是否目标
		}
		/// @brief	8邻域 连通域检测
		/// @param	listTgtPnt，目标点List，生成过程需要图像检测从上到下、从左到右
		/// @param	matDomTag，与图像尺度相同，按像素标记连通域的矩阵
		/// @return	连通域List，每个对象为一个连通域的像素点List，按照连通域像素数从大到小排序
		public List<List<Point>> ConnectedDomainDetect(List<Point> listTgtPnt, DomainTag[,] matDomTag)
		{
			List<List<Point>> listDoms = new List<List<Point>>();//连通域List
			List<int> listDomIndex = new List<int>();//连通域索引List，合并连通域时，只需要修改索引List的值，而不需要修改每个像素的所属连通域
			for (int i = 0; i < listTgtPnt.Count; i++)
			{
				int nX = listTgtPnt[i].X;
				int nY = listTgtPnt[i].Y;
				if (nX >= 1 && matDomTag[nX - 1, nY]._IsTarget)//左侧是目标，则加入左侧组（左侧一定已标记连通域）
				{
					matDomTag[nX, nY]._DomInd = matDomTag[nX - 1, nY]._DomInd;
					listDoms[listDomIndex[matDomTag[nX, nY]._DomInd]].Add(listTgtPnt[i]);
				}
				if (nY >= 1 && matDomTag[nX, nY - 1]._IsTarget)//上方是目标
				{
					if (matDomTag[nX, nY]._DomInd < 0)//自己尚未标记，则加入上方组（上方一定已标记连通域）
					{
						listDoms[listDomIndex[matDomTag[nX, nY - 1]._DomInd]].Add(listTgtPnt[i]);
						matDomTag[nX, nY]._DomInd = matDomTag[nX, nY - 1]._DomInd;
					}
					else if (listDomIndex[matDomTag[nX, nY]._DomInd] != listDomIndex[matDomTag[nX, nY - 1]._DomInd])//自己已标记，则与上方组合并
					{
						listDoms[listDomIndex[matDomTag[nX, nY - 1]._DomInd]].AddRange(listDoms[listDomIndex[matDomTag[nX, nY]._DomInd]]);//合并到上方连通域
						listDoms[listDomIndex[matDomTag[nX, nY]._DomInd]].Clear();//清除自己的连通域
						listDomIndex[matDomTag[nX, nY]._DomInd] = listDomIndex[matDomTag[nX, nY - 1]._DomInd];//索引合并
					}
				}
				if (nX >= 1 && nY >= 1 && matDomTag[nX - 1, nY - 1]._IsTarget)//左上是目标
				{
					if (matDomTag[nX, nY]._DomInd < 0)//自己尚未标记，则加入左上组（左上一定已标记连通域）
					{
						listDoms[listDomIndex[matDomTag[nX - 1, nY - 1]._DomInd]].Add(listTgtPnt[i]);
						matDomTag[nX, nY]._DomInd = matDomTag[nX - 1, nY - 1]._DomInd;
					}
					else if (listDomIndex[matDomTag[nX, nY]._DomInd] != listDomIndex[matDomTag[nX - 1, nY - 1]._DomInd])//自己已标记，则与左上组合并
					{
						listDoms[listDomIndex[matDomTag[nX - 1, nY - 1]._DomInd]].AddRange(listDoms[listDomIndex[matDomTag[nX, nY]._DomInd]]);//合并到左上连通域
						listDoms[listDomIndex[matDomTag[nX, nY]._DomInd]].Clear();//清除自己的连通域
						listDomIndex[matDomTag[nX, nY]._DomInd] = listDomIndex[matDomTag[nX - 1, nY - 1]._DomInd];//索引合并
					}
				}
				if (matDomTag[nX, nY]._DomInd < 0)//最终未标记，则新建连通域
				{
					listDoms.Add(new List<Point>());
					listDomIndex.Add(listDoms.Count - 1);//新加的连通域一定在最后一个
					matDomTag[nX, nY]._DomInd = listDomIndex.Count - 1;
					listDoms[listDoms.Count - 1].Add(listTgtPnt[i]);
				}
			}

			for (int i = listDoms.Count - 1; i >= 0; i--)
			{
				if (listDoms[i].Count == 0)//清除连通域List中无效的项目
					listDoms.RemoveAt(i);
			}
			listDoms.Sort((List<Point> lst1, List<Point> lst2) =>
			{
				if (lst1.Count < lst2.Count)
					return 1;
				else if (lst1.Count > lst2.Count)
					return -1;
				else
					return 0;
			});//按连通域中像素数 从大到小排序

			return listDoms;
		}
		private Rectangle FindJumper(float[,,] matHsv)
		{
			int nHeight = matHsv.GetLength(0);
			int nWidth = matHsv.GetLength(1);

			List<Point> listTgtPnt = new List<Point>();//目标点List
			DomainTag[,] matDomTag = new DomainTag[nWidth, nHeight];//按像素标记连通域的矩阵

			/// 1. 按照HSV阈值标记目标像素
			float[] hsv1 = new float[3] { 215, 0.07f, 0.20f };
			float[] hsv2 = new float[3] { 265, 0.31f, 0.55f };
			for (int i = 0; i < nHeight; ++i)
			{
				for (int j = 0; j < nWidth; ++j)
				{
					matDomTag[j, i]._IsTarget = false;//缺省标记为非目标
					if ((hsv1[0] <= matHsv[i, j, 0] && matHsv[i, j, 0] <= hsv2[0])
						&& (hsv1[1] <= matHsv[i, j, 1] && matHsv[i, j, 1] <= hsv2[1])
						&& (hsv1[2] <= matHsv[i, j, 2] && matHsv[i, j, 2] <= hsv2[2]))
					{
						matDomTag[j, i]._IsTarget = true;//标记为目标
						matDomTag[j, i]._DomInd = -1;//未标记连通域
						listTgtPnt.Add(new Point(j, i));//加入到目标点List

						if (mIfDebug)
							mBmpDebug.SetPixel(j, i, Color.Red);
					}
				}
			}

			/// 2. 8邻域 连通域检测
			List<List<Point>> listDoms= ConnectedDomainDetect(listTgtPnt, matDomTag);//连通域List
			if (listDoms.Count < 2)
				return new Rectangle();

			/// 3. 求Jumper矩形
			Point pntLeftTop = new Point(nWidth, nHeight);
			Point pntRightBot = new Point(0, 0);
			foreach (Point pntPix in listDoms[0])
			{
				if (pntLeftTop.X > pntPix.X)
					pntLeftTop.X = pntPix.X;
				if (pntLeftTop.Y > pntPix.Y)
					pntLeftTop.Y = pntPix.Y;

				if (pntRightBot.X < pntPix.X)
					pntRightBot.X = pntPix.X;
				if (pntRightBot.Y < pntPix.Y)
					pntRightBot.Y = pntPix.Y;
			}
			foreach (Point pntPix in listDoms[1])
			{
				if (pntLeftTop.X > pntPix.X)
					pntLeftTop.X = pntPix.X;
				if (pntLeftTop.Y > pntPix.Y)
					pntLeftTop.Y = pntPix.Y;

				if (pntRightBot.X < pntPix.X)
					pntRightBot.X = pntPix.X;
				if (pntRightBot.Y < pntPix.Y)
					pntRightBot.Y = pntPix.Y;
			}

			return new Rectangle(pntLeftTop.X, pntLeftTop.Y, pntRightBot.X - pntLeftTop.X, pntRightBot.Y - pntLeftTop.Y);
		}
		private Rectangle FindDest(Bitmap bmpSrc, Point pntDest)
		{
			Color clrPix = bmpSrc.GetPixel(pntDest.X, pntDest.Y);//种子点
			List<Point> listTgtPnt = new List<Point>();//目标点List
			DomainTag[,] matDomTag = new DomainTag[bmpSrc.Width, bmpSrc.Height];//按像素标记连通域的矩阵

			/// 1. 按照种子RGB标记目标像素
			for (int i = 0; i < bmpSrc.Height; ++i)
			{
				for (int j = 0; j < bmpSrc.Width; ++j)
				{
					if (bmpSrc.GetPixel(j, i) == clrPix)
					{
						matDomTag[j, i]._IsTarget = true;//标记为目标
						matDomTag[j, i]._DomInd = -1;//未标记连通域
						listTgtPnt.Add(new Point(j, i));//加入到目标点List

						if (mIfDebug)
							mBmpDebug.SetPixel(j, i, Color.Purple);
					}
					else
					{
						matDomTag[j, i]._IsTarget = false;//缺省标记为非目标
					}
				}
			}
			/// 2. 8邻域 连通域检测
			List<List<Point>> listDoms = ConnectedDomainDetect(listTgtPnt, matDomTag);//连通域List
			if (listDoms.Count < 1)
				return new Rectangle();

			/// 3. 求Jumper矩形
			Point pntLeftTop = new Point(bmpSrc.Width, bmpSrc.Height);
			Point pntRightBot = new Point(0, 0);
			foreach (Point pntPix in listDoms[0])
			{
				if (pntLeftTop.X > pntPix.X)
					pntLeftTop.X = pntPix.X;
				if (pntLeftTop.Y > pntPix.Y)
					pntLeftTop.Y = pntPix.Y;

				if (pntRightBot.X < pntPix.X)
					pntRightBot.X = pntPix.X;
				if (pntRightBot.Y < pntPix.Y)
					pntRightBot.Y = pntPix.Y;
			}

			return new Rectangle(pntLeftTop.X, pntLeftTop.Y, pntRightBot.X - pntLeftTop.X, pntRightBot.Y - pntLeftTop.Y);
		}
/*
		private Rectangle FindDest(Bitmap bmpSrc, Point pntDest)
		{
			Color clrPix = bmpSrc.GetPixel(pntDest.X, pntDest.Y);
			Point pntLeftTop = new Point(bmpSrc.Width, bmpSrc.Height);
			Point pntRightBot = new Point(0, 0);
			for (int i = pntDest.X; i >= 0; i--)
			{
				if (bmpSrc.GetPixel(i, pntDest.Y) != clrPix)
				{
					pntLeftTop.X = i+1;
					break;
				}
			}
			for (int i = pntDest.X; i < bmpSrc.Width; i++)
			{
				if (bmpSrc.GetPixel(i, pntDest.Y) != clrPix)
				{
					pntRightBot.X = i - 1;
					break;
				}
			}
			for (int i = pntDest.Y; i >= 0; i--)
			{
				if (bmpSrc.GetPixel(pntDest.X, i) != clrPix)
				{
					pntLeftTop.Y = i + 1;
					break;
				}
			}
			for (int i = pntDest.Y; i < bmpSrc.Height; i++)
			{
				if (bmpSrc.GetPixel(pntDest.X, i) != clrPix)
				{
					pntRightBot.Y = i - 1;
					break;
				}
			}
			return new Rectangle(pntLeftTop.X, pntLeftTop.Y, pntRightBot.X - pntLeftTop.X, pntRightBot.Y - pntLeftTop.Y);
		}
*/
		public Bitmap CutImage(Image imgSrc, Rectangle rctCut)
		{
			Bitmap bmpDst = new Bitmap(rctCut.Width, rctCut.Height);
			Graphics gra = Graphics.FromImage(bmpDst);
			gra.DrawImage(imgSrc, new Rectangle(0, 0, rctCut.Width, rctCut.Height), rctCut, GraphicsUnit.Pixel);
			return bmpDst;
		}
		public static Bitmap MakeGrayScale(Bitmap original)
		{
			//create a blank bitmap the same size as original  
			Bitmap newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);

			//get a graphics object from the new image  
			Graphics g = Graphics.FromImage(newBitmap);

			//create the grayscale ColorMatrix  
			ColorMatrix colorMatrix = new ColorMatrix(
				new float[][]
				{
			new float[] { .3f, .3f, .3f, 0, 0 },
			new float[] { .59f, .59f, .59f, 0, 0 },
			new float[] { .11f, .11f, .11f, 0, 0 },
			new float[] { 0, 0, 0, 1, 0 },
			new float[] { 0, 0, 0, 0, 1 }
				});
			/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
			┌                          ┐ 
			│  0.3   0.3   0.3   0   0 │ 
			│ 0.59  0.59  0.59   0   0 │ 
			│ 0.11  0.11  0.11   0   0 │ 
			│    0     0     0   1   0 │ 
			│    0     0     0   0   1 │ 
			└                          ┘ 
			 * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
			//create some image attributes  
			ImageAttributes attributes = new ImageAttributes();

			//set the color matrix attribute  
			attributes.SetColorMatrix(colorMatrix);

			//draw the original image on the new image  
			//using the grayscale color matrix  
			g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
			   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

			//dispose the Graphics object  
			g.Dispose();
			return newBitmap;
		}
		private int TouchDuration(Point pntStart, Point pntEnd)
		{
			int nDelta = (int)Math.Sqrt(Math.Pow(pntStart.X - pntEnd.X , 2) + Math.Pow(pntStart.Y - pntEnd.Y , 2));
// 			int nDelta = (int)(Math.Abs(pntStart.X - pntEnd.X) * 2 / 1.732);
			return (int)(nDelta * numStepRatio.Value *1280/ mImgScreen.Height);
		}
		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Enabled = false;
			Routine();
		}
		public float[,,]RGB2HSV(Bitmap bmpRGB)
		{
			float[,,] matHsv = new float[bmpRGB.Height, bmpRGB.Width, 3];
			for (int i = 0; i < bmpRGB.Height; ++i)
			{
				for (int j = 0; j < bmpRGB.Width; j++)
				{
					Color rgbPix = bmpRGB.GetPixel(j, i);
					matHsv[i, j, 0] = rgbPix.GetHue();
					matHsv[i, j, 1] = rgbPix.GetSaturation();
					matHsv[i, j, 2] = rgbPix.GetBrightness();
				}
			}
			return matHsv;
		}
	}
}
