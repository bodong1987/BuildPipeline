import sys
import os

_my_dir = os.path.dirname(os.path.realpath(__file__))
_lib_dir = os.path.join(_my_dir, "../../../scripts")
sys.path.append(_lib_dir)

from baselib import task_definitions

task_def = task_definitions.Task("test", 1024, "Python Test Task", Condition="")
task_def.add_argument("boolVar", "Boolean Variable", True, Require = True)
task_def.add_argument("intVar", "Int32 Variable", 1024, Require = False)
task_def.add_argument("strVar", "String Variable", "String Text", Require = False)
task_def.add_argument("listVar", "List Variable", ["list value0", "list value1"], Require=False)

if __name__ == "__main__" :
    args = task_def.parse_command()
    print("--boolVar = " + str(args.boolVar))
    print("--intVar = " + str(args.intVar))
    print("--strVar = " + str(args.strVar))
    print("--listVar = " + str(args.listVar))