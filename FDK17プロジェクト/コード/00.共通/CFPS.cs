using System;
using System.Collections.Generic;

namespace FDK
{
	public class CFPS
	{
		// プロパティ

		public int n現在のFPS
		{
			get;
			private set;
		}

		// コンストラクタ

		public CFPS()
		{
			this.n現在のFPS = 0;
			this.timer = new CTimer(CTimer.E種別.MultiMedia);

			StartNewReportingInterval();

			_baseTimeAtLastReset = this.基点時刻ms;
			_updatesSinceLastReset = 0;
		}

		private void StartNewReportingInterval()
		{
			this.基点時刻ms = this.timer.n現在時刻;
			this._updatesSinceLastReportingInterval = 0;
		}

		// メソッド

		public void tカウンタ更新()
		{
			this.timer.t更新();
			this._updatesSinceLastReportingInterval++;
			this._updatesSinceLastReset++;

			const double REPORTING_INVERVAL_MS = 5000;
			var interval = this.timer.n現在時刻 - this.基点時刻ms;
			if (REPORTING_INVERVAL_MS <= interval)
			{
				this.n現在のFPS = (int)(_updatesSinceLastReportingInterval / (interval / 1000.0));
				_averageFpsAtReportingIntervalsSinceLastReset.Add(this.n現在のFPS); 
				StartNewReportingInterval();
			}
		}

		public AverageFpsInformation GetAndResetAverageFpsSinceLastReset()
		{
			long n現在時刻 = this.timer.n現在時刻;
			var interval = n現在時刻 - _baseTimeAtLastReset;

			var averageFpsSinceLastReset = (int)(_updatesSinceLastReset / (interval / 1000.0));

			// old-school copy for .net 3.5 until upgrade
			var averageFpsAtReportingIntervalsSinceLastReset = new int[_averageFpsAtReportingIntervalsSinceLastReset.Count];
			_averageFpsAtReportingIntervalsSinceLastReset.CopyTo(averageFpsAtReportingIntervalsSinceLastReset);

			_baseTimeAtLastReset = n現在時刻;
			_updatesSinceLastReset = 0;
			_averageFpsAtReportingIntervalsSinceLastReset.Clear();
			return new AverageFpsInformation(averageFpsSinceLastReset, averageFpsAtReportingIntervalsSinceLastReset);
		}

		// その他

		#region [ private ]
		//-----------------
		private CTimer	timer;
		private long	基点時刻ms;
		private long	_updatesSinceLastReportingInterval;
		private long	_baseTimeAtLastReset;
		private long _updatesSinceLastReset;
		private List<int> _averageFpsAtReportingIntervalsSinceLastReset = new List<int>();

		public class AverageFpsInformation
		{
			public int AverageFpsSinceLastReset { get; }
			public int[] AverageFpsAtReportingIntervalsSinceLastReset { get; }

			public AverageFpsInformation(int averageFpsSinceLastReset, int[] averageFpsAtReportingIntervalsSinceLastReset)
			{
				AverageFpsSinceLastReset = averageFpsSinceLastReset;
				AverageFpsAtReportingIntervalsSinceLastReset = averageFpsAtReportingIntervalsSinceLastReset;
			}
		}

		//-----------------
		#endregion
	}
}
