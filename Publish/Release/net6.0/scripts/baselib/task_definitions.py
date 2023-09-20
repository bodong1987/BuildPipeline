# used to share code between IronPython and native python
import argparse

class TaskOption :
    Name = None
    DisplayName = None
    Description = None
    Require = None
    Default = None

    def __init__(self) -> None:
        self.Name = ""
        self.DisplayName = ""
        self.Description = ""
        self.Require = False
    
class Task :
    Options = None
    Parser = None
    Condition = None

    def __init__(self, name, order, description, **kwargs):
        self.Name = name
        self.Order = order
        self.Description = description
        self.Options = []
        self.Parser = argparse.ArgumentParser(prog=name, description=description)

        if("Condition" in kwargs) :
            self.Condition = kwargs["Condition"]

    def parse_command(self) :
        args, unknwon = self.Parser.parse_known_args()
        return args

    # additonal params
    # Description : detail help text
    # Require : is this option must exists
    def add_argument(self, name, display_name, default_value, **kwargs) :
        option = TaskOption()
        option.Name = name
        option.DisplayName = display_name
        option.Default = default_value

        if("Description" in kwargs) :
            option.Description = kwargs["Description"]

        if("Require" in kwargs) :
            option.Require = kwargs["Require"]

        self.Options.append(option)

        self.Parser.add_argument(
                "--" + option.Name,
                help = option.Description,
                required = option.Require,
                default = option.Default,
                nargs = '*' if type(option.Default) == list else None
                )

    def get_settings(self) :
        import clr
        clr.AddReference("BuildPipeline.Core")

        from BuildPipeline.Core.BuilderFramework.Scriptable import ScriptableBuildTaskSettings, ScriptableBuildTaskOptions, ScriptableObject
        from BuildPipeline.Core.BuilderFramework import EnvironmentServiceRequirement, ServiceRequirement
        from BuildPipeline.Core.Services import IPythonEnvironmentService, IExternalProcessService

        settings = ScriptableBuildTaskSettings.CreateSettings(self.Name, self.Description, self.Order)

        # add default requirements
        pythonRequirement = EnvironmentServiceRequirement[IPythonEnvironmentService]("3.4")
        settings.Requirements.Require(ServiceRequirement[IExternalProcessService]())
        settings.Requirements.Require(pythonRequirement)

        # set condition if need
        if(self.Condition != None) :
            settings.ActiveCondition = str(self.Condition)

        # add options
        for option in self.Options :
            property = ScriptableObject()
            property.Name = option.Name
            property.DisplayName = option.DisplayName if option.DisplayName != None else option.Name
            property.Description = option.Description if option.Description != None else property.DisplayName 
            property.Value = option.Default
            property.Require = option.Require

            settings.Options.AddProperty(property)

        return settings