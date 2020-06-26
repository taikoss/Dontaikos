using FDK;
using System;
using System.Windows.Forms;
using SampleFramework;
using System.Collections.Generic;
using System.Diagnostics;


namespace TJAPlayer3
{
	internal class CStage起動 : CStage
	{
		// コンストラクタ

		public CStage起動()
		{
			base.eステージID = CStage.Eステージ.起動;
			base.b活性化してない = true;
		}

		public List<string> list進行文字列;

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation("起動ステージを活性化します。");
			Trace.Indent();
			try
			{
				RandomEvent = rng.Next(0, 1);
				//伯方の塩
				if (RandomEvent == 0)
				{
					RandomTx = new CTexture[1];

					RandomTx[0] = TJAPlayer3.Tx.TxC(@"0_Startup\hakata.png");
					RandomSE[0] = new CSkin.Cシステムサウンド(@"Sounds\Startup\hakata.ogg", false, false, ESoundGroup.SongPlayback);
				}

				this.list進行文字列 = new List<string>();
				base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
				base.On活性化();
				Trace.TraceInformation("起動ステージの活性化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			Trace.TraceInformation("起動ステージを非活性化します。");
			Trace.Indent();
			try
			{
				TJAPlayer3.t安全にDisposeする(RandomTx);

				this.list進行文字列 = null;
				if (es != null)
				{
					if ((es.thDTXFileEnumerate != null) && es.thDTXFileEnumerate.IsAlive)
					{
						Trace.TraceWarning("リスト構築スレッドを強制停止します。");
						es.thDTXFileEnumerate.Abort();
						es.thDTXFileEnumerate.Join();
					}
				}
				base.On非活性化();
				Trace.TraceInformation("起動ステージの非活性化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				this.tx背景 = TJAPlayer3.tテクスチャの生成(CSkin.Path(@"Graphics\1_Title\Background.png"), false);
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				TJAPlayer3.t安全にDisposeする(ref this.tx背景);
				base.OnManagedリソースの解放();
			}
		}

		public void t読込開始()
		{
			bLoadStarted = true;

			var random = rng.Next(0, 100);

			if (random >= 0 && random < 99) this.bgm起動画面 = new CSkin.Cシステムサウンド(@"Sounds\Startup\BGM_1.ogg", true, false, ESoundGroup.SongPlayback);
			else if (random == 99) this.bgm起動画面 = new CSkin.Cシステムサウンド(@"Sounds\Startup\BGM_2.ogg", true, false, ESoundGroup.SongPlayback);

			this.bgm起動画面.t再生する();

			this.list進行文字列.Add(TJAPlayer3.AppDisplayNameWithInformationalVersion);
			this.list進行文字列.Add("");
			this.list進行文字列.Add("This is a fork of twopointzero/TJAPlayer3.");
			this.list進行文字列.Add("");
			this.list進行文字列.Add($"{TJAPlayer3.AppDisplayName} is open source software under the MIT license.");
			this.list進行文字列.Add("See README for acknowledgments.");
			this.list進行文字列.Add("");

			es = new CEnumSongs();
			es.StartEnumFromCache(); // 曲リスト取得(別スレッドで実行される)
		}

		public override int On進行描画()
		{
			if (!base.b活性化してない)
			{
				if (!bLoadStarted)
				{
					if (base.b初めての進行描画)
					{
						this.RandomTimer = new CCounter(0, 1000, 10, TJAPlayer3.Timer);
					}

					if (RandomEvent == 0)
					{
						if (base.b初めての進行描画) this.RandomSE[0].t再生する();

						this.RandomTx[0].t2D描画(TJAPlayer3.app.Device, 0, 0);

						if (this.RandomTimer.n現在の値 >= 250)
						{
							this.t読込開始();
						}
					}

					this.RandomTimer.t進行();
				}

				if (base.b初めての進行描画)
				{
					//this.t読込開始();
					
					base.b初めての進行描画 = false;
				}

				// CSongs管理 s管理 = CDTXMania.Songs管理;

				//if( this.tx背景 != null )
				//	this.tx背景.t2D描画( CDTXMania.app.Device, 0, 0 );

				if (bLoadStarted)
				{
					#region [ this.str現在進行中 の決定 ]
					//-----------------
					switch (base.eフェーズID)
					{
						case CStage.Eフェーズ.起動0_システムサウンドを構築:
							this.str現在進行中 = "SYSTEM SOUND...";
							break;

						case CStage.Eフェーズ.起動00_songlistから曲リストを作成する:
							this.str現在進行中 = "SONG LIST...";
							break;

						case CStage.Eフェーズ.起動1_SongsDBからスコアキャッシュを構築:
							this.str現在進行中 = "SONG DATABASE...";
							break;

						case CStage.Eフェーズ.起動2_曲を検索してリストを作成する:
							this.str現在進行中 = string.Format("{0} ... {1}", "Enumerating songs", es.Songs管理.n検索されたスコア数);
							break;

						case CStage.Eフェーズ.起動3_スコアキャッシュをリストに反映する:
							this.str現在進行中 = string.Format("{0} ... {1}/{2}", "Loading score properties from songs.db", es.Songs管理.nスコアキャッシュから反映できたスコア数, es.Songs管理.n検索されたスコア数);
							break;

						case CStage.Eフェーズ.起動4_スコアキャッシュになかった曲をファイルから読み込んで反映する:
							this.str現在進行中 = string.Format("{0} ... {1}/{2}", "Loading score properties from files", es.Songs管理.nファイルから反映できたスコア数, es.Songs管理.n検索されたスコア数 - es.Songs管理.nスコアキャッシュから反映できたスコア数);
							break;

						case CStage.Eフェーズ.起動5_曲リストへ後処理を適用する:
							this.str現在進行中 = string.Format("{0} ... ", "Building songlists");
							break;

						case CStage.Eフェーズ.起動6_スコアキャッシュをSongsDBに出力する:
							this.str現在進行中 = string.Format("{0} ... ", "Saving songs.db");
							break;

						case CStage.Eフェーズ.起動7_完了:
							this.list進行文字列.Add("LOADING TEXTURES...");
							TJAPlayer3.Tx.LoadTexture();
							this.list進行文字列.Add("LOADING TEXTURES...OK");
							this.str現在進行中 = "Setup done.";
							break;
					}
					//-----------------
					#endregion
					#region [ this.list進行文字列＋this.現在進行中 の表示 ]
					//-----------------
					lock (this.list進行文字列)
					{
						int x = 320;
						int y = 20;
						foreach (string str in this.list進行文字列)
						{
							TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.Eフォント種別.白, str);
							y += 24;
						}
						TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.Eフォント種別.白, this.str現在進行中);
					}
					//-----------------
					#endregion

					if (es != null && es.IsSongListEnumCompletelyDone)                          // 曲リスト作成が終わったら
					{
						TJAPlayer3.Songs管理 = (es != null) ? es.Songs管理 : null;      // 最後に、曲リストを拾い上げる

						this.bgm起動画面.t停止する();
						return 1;
					}
				}
			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private string str現在進行中 = "";
		private CTexture tx背景;
		private CEnumSongs es;

		private bool bLoadStarted;

		private Random rng = new System.Random();
		public CSkin.Cシステムサウンド bgm起動画面 = null;
		private int RandomEvent;
		private CCounter RandomTimer;

		//random textures
		private CTexture[] RandomTx;
		private CSkin.Cシステムサウンド[] RandomSE = new CSkin.Cシステムサウンド[10];

		#endregion
	}
}