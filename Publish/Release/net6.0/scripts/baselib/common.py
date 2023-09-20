# for base methods
# python version must be : 3.4.0
# new version python feature is not supported.
import sys
import os
import argparse
import shutil
import shutil
from pathlib import Path
import pathlib

# common script directory
common_script_directory_path = os.path.dirname(os.path.realpath(__file__))

# project_directory
project_directory = str(Path(common_script_directory_path).parent.parent)

# get file name with extension
def get_file_name(path) :
    return os.path.basename(path)

# get file name without extension
def get_file_name_without_extension(path) :
    file_name = get_file_name(path)
    bname, _ = os.path.splitext(file_name)
    return bname

#get file path extension
def get_file_extension(path) :
    file_name = get_file_name(path)
    _, extension = os.path.splitext(file_name)
    return extension
    
#get directory
def get_directoy_path(path) :
    return str(Path(path).parent)

#get relative path
def get_relative_path(path, from_path) :
    return os.path.relpath(path, from_path)

#convert path to system standard path, remove relative path...
def get_canonical_path(path) :
    return os.path.realpath(path)

#get all files with extensions
def get_files_recursively(directory, *extensions) :
    targets = []

    for root, _, files in os.walk(directory) :        
        for f in files :
            file = os.path.join(root, f)
            if(any(file.endswith(ext) for ext in extensions)) :
                targets.append(file)
    return targets

#ge all files without extensions
def get_all_files_recursively(directory) :
    targets = []

    for root, _, files in os.walk(directory) :        
        for f in files :
            file = os.path.join(root, f)
            targets.append(file)
    return targets

def str2bool(v):
    if isinstance(v, bool):
       return v
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Boolean value expected.')

# check copy file is needed?
def check_need_update(source_file, target_file) :
    if(os.path.exists(source_file) and os.path.exists(target_file)) :
        st = pathlib.Path(source_file).stat().st_mtime
        tt = pathlib.Path(target_file).stat().st_mtime

        return st > tt
    return True

# copy file
def copy_file(source_file, target_file) :
    dir = get_directoy_path(target_file) 
    if(not os.path.exists(dir)) :
        os.makedirs(dir)

    print("@info copy " + source_file + " to " + target_file + " ...")
    shutil.copyfile(source_file, target_file)

# checked copy
def checked_copy_file(source_file, target_file) :
    if(check_need_update(source_file, target_file)) :
        copy_file(source_file, target_file)

#copy with array filter
def copy_directory_with_filter(source, dest, filter) :
    """Copy a directory structure overwriting existing files"""
    for root, _, files in os.walk(source):
        if not os.path.isdir(root):
            os.makedirs(root)

        for file in files:
            rel_path = root.replace(source, '').lstrip(os.sep)

            dest_path = os.path.join(dest, rel_path)

            source_path = os.path.join(root, file)
            should_skip = False
            if(filter != None) :
                for key in filter :
                    if(key in source_path) :
                        should_skip = True
                        break
            
            if(should_skip) :
                continue

            if not os.path.isdir(dest_path):
                os.makedirs(dest_path)
            try :                
                checked_copy_file(os.path.join(root, file), os.path.join(dest_path, file))
            except :
                # some times, visual studio will lock these files, but you don't want to close visual studio...
                print("Failed copy " + os.path.join(root, file) + " to " + os.path.join(dest_path, file) + ".")

# ignore_directories the path relative to root
def copy_directory(source, dest):
    copy_directory_with_filter(source, dest, None)
