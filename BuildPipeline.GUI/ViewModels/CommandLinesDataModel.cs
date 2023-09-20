using BuildPipeline.Core.BuilderFramework;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;

namespace BuildPipeline.GUI.ViewModels
{
    /// <summary>
    /// Class CommandLineDataModel.
    /// </summary>
    public class CommandLineDataModel 
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the command description.
        /// </summary>
        /// <value>The command description.</value>
        public string CommandDescription { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineDataModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        public CommandLineDataModel(string name, string command)
        {
            Name = name;
            CommandDescription = command;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Class CommandLineDataModel.
    /// Implements the <see cref="ReactiveObject" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    public class CommandLinesDataModel : ReactiveObject
    {
        readonly List<CommandLineDataModel> ItemsCore = new List<CommandLineDataModel>();

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>The items.</value>
        public CommandLineDataModel[] Items => ItemsCore.ToArray();

        CommandLineDataModel SelectedItemCore;
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public CommandLineDataModel SelectedItem
        {
            get => SelectedItemCore;
            set => this.RaiseAndSetIfChanged(ref SelectedItemCore, value);
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        [DependsOnProperty(nameof(SelectedItem))]
        public string Command => SelectedItem?.CommandDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLinesDataModel"/> class.
        /// </summary>
        public CommandLinesDataModel()
        {
            Add("All", BuildFramework.GetCommandLineHelpText());

            foreach(var factory in BuildFramework.AllFactories)
            {
                var command = BuildFramework.GetCommandLineHelpText(factory.Name);

                Add(factory.Name, command);
            }

            SelectedItem = ItemsCore.FirstOrDefault();
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        public void Add(string name, string command)
        {
            ItemsCore.Add(new CommandLineDataModel(name, command));
        }
    }
}
