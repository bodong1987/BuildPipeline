using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Extensions.IO;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Utils;
using PropertyModels.Extensions;
using System.Text;

namespace BuildPipeline.Core.Services.Implements
{
    /// <summary>
    /// Class CompressService.
    /// Implements the <see cref="AbstractService" />
    /// Implements the <see cref="BuildPipeline.Core.Services.ICompressService" />
    /// </summary>
    /// <seealso cref="AbstractService" />
    /// <seealso cref="BuildPipeline.Core.Services.ICompressService" />
    [Export]
    internal class CompressService : AbstractService, ICompressService
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public override string ServiceName => "Compress Service";

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// 获取当前服务是否在线
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        public override bool IsAvailable
        {
            get
            {
                var externalProcessService = ServiceProvider.GetService<IExternalProcessService>();

                if(externalProcessService == null)
                {
                    return false;
                }

                ISevenZipEnvironmentService service = ServiceProvider.GetService<ISevenZipEnvironmentService>();
                
                return service.IsAvailable;
            }
        }

        /// <summary>
        /// Compresses the files.
        /// 压缩inputList数组中指定的所有内容
        /// 输出文件扩展名是7z就按照7z压缩 是zip就按照zip压缩
        /// 注意inputList是一个数组 里面可以放文件路径，也可以放文件夹
        /// </summary>
        /// <param name="intputList">The intput list.</param>
        /// <param name="archiveFile">The archive file.</param>
        /// <param name="observer">The observer.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Compress(
            string[] intputList, 
            string archiveFile, 
            ICompressServiceObserver observer, 
            CancellationTokenSource cancellationTokenSource,
            string password = null
            )
        {
            if (!IsAvailable)
            {
                Logger.LogError("7zip is not found.");
                return false;
            }

            ISevenZipEnvironmentService service = ServiceProvider.GetService<ISevenZipEnvironmentService>();

            Logger.Assert(service != null && service.IsAvailable);

            if (archiveFile.IsFileExists())
            {
                try
                {
                    File.Delete(archiveFile);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed delete file {0}\n{1}", archiveFile, ex.Message);
                    return false;
                }
            }

            // gen template files 
            var fileListPath = Path.Combine(AppFramework.ApplicationTempDirectory, $"BuilePipeline_Compress_{Guid.NewGuid()}.txt");
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var f in intputList)
            {
                stringBuilder.AppendLine(f);
                Logger.Log("---- Compress Target: {0}", f);
            }

            File.WriteAllText(fileListPath, stringBuilder.ToString());
            string passwordArguments = password.IsNullOrEmpty() ? "" : $" -p\"{password}\"";

            var task = ServiceProvider.GetService<IExternalProcessService>().StartAsync(
                service.SevenZipPath,
                $" a \"{archiveFile}\" @\"{fileListPath}\" {passwordArguments}",
                true,
                new ExternalProcessEventObserver(),
                cancellationTokenSource
                );

            task.Wait();

            return task.Result == 0;
        }

    }
}
