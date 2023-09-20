import sys
import os
import os.path
import subprocess
import shutil
import argparse
from pathlib import Path
import pathlib

parser = argparse.ArgumentParser(description="post build script, used to copy assembly to target path")
parser.add_argument("--targetDir", help="$(TargetDir)", required=True)
parser.add_argument("--projectName", help="$(ProjectName)", required=True)
parser.add_argument("--projectDir", help="$(ProjectDir)", required=True)
parser.add_argument("--configurationName", help="$(ConfigurationName)", required=True)
parser.add_argument("--outputType", help="$(OutputType)", required=True)

args, unknown = parser.parse_known_args()

_target_dir = str(args.targetDir)
_project_name = str(args.projectName)
_project_dir = str(args.projectDir)
_configuration = str(args.configurationName)
_outputType = str(args.outputType)

print(sys.argv[1:])

def _get_canonical_path(path) :
    return os.path.realpath(path)

def _get_file_name(path) :
    return os.path.basename(path)

def _get_file_name_without_extension(path) :
    file_name = _get_file_name(path)
    bname, _ = os.path.splitext(file_name)
    return bname

def _get_framework_name() :
    return _get_file_name(_get_canonical_path(_target_dir))

def _check_debug_build() :
    if("debug" in _configuration.lower()) :
        return True
    return False

_current_path = os.path.dirname(os.path.realpath(__file__))
_project_root_directory = _get_canonical_path(os.path.join(_current_path, "../"))

def _get_publish_path() :    
    if(_check_debug_build()) :
        return os.path.join(_project_root_directory, "Publish/Debug", _get_framework_name() )
    else :
        return os.path.join(_project_root_directory, "Publish/Release", _get_framework_name())

def _get_plugin_name(project_name) :    
    if(project_name.startswith("BuildPipeline.Plugins.")) :
        return project_name[len("BuildPipeline.Plugins."):]
    return project_name

def _check_need_update(source_file, target_file) :
    if(os.path.exists(source_file) and os.path.exists(target_file)) :
        st = pathlib.Path(source_file).stat().st_mtime
        tt = pathlib.Path(target_file).stat().st_mtime

        return st > tt
    return True

#get directory
def _get_directoy_path(path) :
    return str(Path(path).parent)

def _copy_file(source_file, target_file) :
    dir = _get_directoy_path(target_file) 
    if(not os.path.exists(dir)) :
        os.makedirs(dir)

    print("@info copy " + source_file + " to " + target_file + " ...")
    shutil.copyfile(source_file, target_file)


# checked copy
def _checked_copy_file(source_file, target_file) :
    if(_check_need_update(source_file, target_file)) :
        _copy_file(source_file, target_file)

def _copy_plugin_to_dest_plugins_path(project_name, target_path) :
    output_dir = _get_publish_path()
    output_plugins = os.path.join(target_path, "plugins", _get_plugin_name(project_name))

    exts = [".dll", ".deps.json", ".pdb", ".xml"]
    for ext in exts :
        source_path = os.path.join(_target_dir, project_name + ext)
        dest_path = os.path.join(output_plugins, project_name + ext)
        if(os.path.exists(source_path)):
            _checked_copy_file(source_path, dest_path)

def _check_exe_project() :
    return "exe" in _outputType.lower()

def _check_plugin_project() :
    if(sys.platform == "win32") :
        return "\\plugins\\" in _project_dir.lower()
    else:
        return "/plugins/" in _project_dir.lower()

_publish_directory = _get_publish_path()
_is_plugin_project = _check_plugin_project()

def _copy_plugin_to_dest_path(project_name) :
    _copy_plugin_to_dest_plugins_path(project_name, _publish_directory)
    
if(_is_plugin_project) :
    _copy_plugin_to_dest_path(_project_name)

