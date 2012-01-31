using System.Threading;

using Banshee.ServiceStack;

using Hyena.Jobs;

namespace Banshee.GoogleMusic
{
	public class AsyncUserJob : UserJob
	{	
		public delegate void DelegateJob();
		
		private DelegateJob callback;
		
		public AsyncUserJob (DelegateJob callback, string title) : base(title)
		{
			this.callback = callback;
			CanCancel = false;
			SetResources(Resource.Cpu);
		}
		
		protected override void RunJob ()
		{
			ThreadPool.QueueUserWorkItem((state) => {
				callback();
				Finish();
			});
		}
		
		public static void Create(DelegateJob callback, string title)
		{
			var job = new AsyncUserJob(callback, title);
			job.Register();
		}
	}
}

