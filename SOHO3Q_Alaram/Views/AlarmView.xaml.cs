using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ToolSetsCore;
using System.Linq;
using Models;
using SOHO3Q_Alaram.ViewModels;

namespace SOHO3Q_Alaram
{
    [Export]
    [Export("AlarmView", typeof(AlarmView))]
    public partial class AlarmView : ToolSetViewBase
    {

        public AlarmView()
        {
            InitializeComponent();
        }

        [Import]
        private AlarmVM ImportVM
        {
            set { VM = value; }
        }
    }
}