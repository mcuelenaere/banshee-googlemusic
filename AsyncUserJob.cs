using System.Threading;

using Banshee.ServiceStack;

using Hyena.Jobs;

namespace Banshee.GoogleMusic
{
	public class AsyncUserJob : UserJob
	{	
		public delegate void DelegateJob();
		
		private Thread thread;
		
		private AsyncUserJob (DelegateJob callback, string title) : base(title)
		{
			thread = new Thread(delegate() {
				callback();
				Finish();
			});
			CanCancel = true;
			CancelRequested += delegate {
				thread.Abort();
				Finish();
				
			};
			SetResources(Resource.Cpu);
		}
		
		protected override void RunJob ()
		{
			thread.Start();
		}
		
		public static void Create(DelegateJob callback, string title)
		{
			var job = new AsyncUserJob(callback, title);
			job.Register();
		}
	}
}

