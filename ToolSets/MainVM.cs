using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;
using ToolSetsCore;
using System.Collections.ObjectModel;

namespace ToolSets
{
    [Export]
    [Export("MainVM", typeof(ToolSetVMBase))]
    public class MainVM : ToolSetVMBase
    {
      
    }
}