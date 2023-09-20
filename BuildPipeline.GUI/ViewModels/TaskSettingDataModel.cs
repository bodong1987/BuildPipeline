using BuildPipeline.Core;
using BuildPipeline.Core.BuilderFramework;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Framework;
using BuildPipeline.Core.Services;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class TaskSettingDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class TaskSettingDataModel : ReactiveObject
    {
        /// <summary>
        /// Gets the context view model.
        /// </summary>
        /// <value>The context view model.</value>
        public NewBuildContextViewModel ContextViewModel { get; private set; }

        IBuildTaskSettings SettingsCore;
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public IBuildTaskSettings Settings
        {
            get => SettingsCore;
            set => this.RaiseAndSetIfChanged(ref SettingsCore, value);
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public IBuildContext Context => ContextViewModel.Context;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string TaskName => Settings.TaskName;

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public string TaskDescription => ServiceProvider.GetService<ILocalizeService>()[Settings.TaskDescription];

        /// <summary>
        /// Gets the task setting group.
        /// </summary>
        /// <value>The task setting group.</value>
        public string TaskSettingGroup => $"{TaskName} IncludeState";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is automatic.
        /// </summary>
        /// <value><c>true</c> if this instance is automatic; otherwise, <c>false</c>.</value>
        public bool IsAuto
        {
            get
            {
                return (Context.TaskNames == null || !Context.TaskNames.Contains(TaskName)) &&
                    (Context.ExcludeTaskNames == null || !Context.ExcludeTaskNames.Contains(TaskName));
            }
            set
            {
                if(value)
                {
                    if (Context.TaskNames != null && Context.TaskNames.Contains(TaskName))
                    {
                        var list = Context.TaskNames.ToList();
                        list.RemoveAll(x => x == TaskName);
                        Context.TaskNames = list.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.TaskNames));
                    }

                    if (Context.ExcludeTaskNames != null && Context.ExcludeTaskNames.Contains(TaskName))
                    {
                        var list = Context.ExcludeTaskNames.ToList();
                        list.RemoveAll(x => x == TaskName);
                        Context.ExcludeTaskNames = list.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.ExcludeTaskNames));
                    }
                }
                else
                {
                    IsInclude = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is include.
        /// </summary>
        /// <value><c>true</c> if this instance is include; otherwise, <c>false</c>.</value>
        public bool IsInclude
        {
            get
            {
                return Context.TaskNames != null && Context.TaskNames.Contains(TaskName);
            }
            set
            {
                if(value)
                {
                    if(Context.TaskNames != null)
                    {
                        if(!Context.TaskNames.Contains(TaskName))
                        {
                            var list = Context.TaskNames.ToList();
                            list.Add(TaskName);
                            Context.TaskNames = list.ToBindingList();
                            Context.RaisePropertyChanged(nameof(Context.TaskNames));
                        }
                    }
                    else
                    {
                        Context.TaskNames = new string[] { TaskName }.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.TaskNames));
                    }
                }
                else
                {
                    if (Context.TaskNames != null && Context.TaskNames.Contains(TaskName))
                    {
                        var list = Context.TaskNames.ToList();
                        list.RemoveAll(x => x == TaskName);
                        Context.TaskNames = list.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.TaskNames));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is exclude.
        /// </summary>
        /// <value><c>true</c> if this instance is exclude; otherwise, <c>false</c>.</value>
        public bool IsExclude
        {
            get
            {
                return Context.ExcludeTaskNames != null && Context.ExcludeTaskNames.Contains(TaskName);
            }
            set
            {
                if(value)
                {
                    if (Context.TaskNames != null && Context.TaskNames.Contains(TaskName))
                    {
                        var list = Context.TaskNames.ToList();
                        list.RemoveAll(x => x == TaskName);
                        Context.TaskNames = list.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.TaskNames));
                    }

                    if (Context.ExcludeTaskNames != null)
                    {
                        if (!Context.ExcludeTaskNames.Contains(TaskName))
                        {
                            var list = Context.ExcludeTaskNames.ToList();
                            list.Add(TaskName);
                            Context.ExcludeTaskNames = list.ToBindingList();
                            Context.RaisePropertyChanged(nameof(Context.ExcludeTaskNames));
                        }
                    }
                    else
                    {
                        Context.ExcludeTaskNames = new string[] { TaskName }.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.ExcludeTaskNames));
                    }
                }
                else
                {
                    if (Context.ExcludeTaskNames != null && Context.ExcludeTaskNames.Contains(TaskName))
                    {
                        var list = Context.ExcludeTaskNames.ToList();
                        list.RemoveAll(x => x == TaskName);
                        Context.ExcludeTaskNames = list.ToBindingList();
                        Context.RaisePropertyChanged(nameof(Context.ExcludeTaskNames));
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSettingDataModel" /> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="settings">The settings.</param>
        public TaskSettingDataModel(NewBuildContextViewModel viewModel, IBuildTaskSettings settings)
        {
            ContextViewModel = viewModel;
            Settings = settings;
        }
    }
}
