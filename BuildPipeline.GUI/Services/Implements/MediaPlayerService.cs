using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using LibVLCSharp.Shared;

namespace BuildPipeline.GUI.Services.Implements
{
    [Export]
    internal class MediaPlayerService : AbstractService, IMediaPlayerService
    {
        private LibVLC Vlc;
        private bool IsInitialized;
        private bool HasInitialError;

        public override bool IsAvailable => IsInitialized && !HasInitialError;

        public override bool Accept(object accessToken)
        {
            // vlc can't support arm macos now
            return !(AppFramework.IsMacOS && AppFramework.IsArm);
        }

        public MediaPlayerService() 
        {
            if(AppFramework.IsMacOS && AppFramework.IsArm)
            {
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    Vlc = new LibVLC(enableDebugLogs: false);
                }
                catch(Exception e)
                {
                    Logger.LogError("Failed initialize vlc. {0}", e.Message);
                    HasInitialError = true;
                }
                
                IsInitialized = true;
            });
        }

        public async Task PlayAudioAsync(string path)
        {
            if(!IsAvailable)
            {
                return;
            }

            if(!path.IsFileExists())
            {
                Logger.LogWarning("Failed play audio, file not exists:{0}", path);

                return;
            }

            MediaPlayer player = null;
            await Task.Run(() =>
            {
                var media = new Media(this.Vlc, path);
                player = new MediaPlayer(media);
            });

            if(player != null)
            {
                player.Play();
            }
        }
    }
}
