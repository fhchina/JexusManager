// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace JexusManager.Features.RequestFiltering
{
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    using JexusManager.Properties;
    using JexusManager.Services;

    using Microsoft.Web.Administration;
    using Microsoft.Web.Management.Client;
    using Microsoft.Web.Management.Client.Win32;

    using Module = Microsoft.Web.Management.Client.Module;

    internal class FileExtensionsFeature : RequestFilteringFeature<FileExtensionsItem>
    {
        private sealed class FeatureTaskList : DefaultTaskList
        {
            private readonly FileExtensionsFeature _owner;

            public FeatureTaskList(FileExtensionsFeature owner)
            {
                _owner = owner;
            }

            public override ICollection GetTaskItems()
            {
                var result = new ArrayList();

                result.Add(new MethodTaskItem("AddExtension", "Allow File Name Extension...", string.Empty).SetUsage());
                result.Add(
                    new MethodTaskItem("AddDenyExtension", "Deny File Name Extension...", string.Empty).SetUsage());
                if (_owner.SelectedItem != null)
                {
                    result.Add(new MethodTaskItem(string.Empty, "-", string.Empty).SetUsage());
                    result.Add(RemoveTaskItem);
                }

                return result.ToArray(typeof(TaskItem)) as TaskItem[];
            }

            [Obfuscation(Exclude = true)]
            public void AddExtension()
            {
                _owner.Add();
            }

            [Obfuscation(Exclude = true)]
            public void AddDenyExtension()
            {
                _owner.AddDeny();
            }

            [Obfuscation(Exclude = true)]
            public override void Remove()
            {
                _owner.Remove();
            }
        }

        public FileExtensionsFeature(Module module)
            : base(module)
        {
        }

        private TaskList _taskList;

        public override TaskList GetTaskList()
        {
            return _taskList ?? (_taskList = new FeatureTaskList(this));
        }

        public void Add()
        {
            this.CreateExtension(true);
        }

        public void AddDeny()
        {
            this.CreateExtension(false);
        }

        private void CreateExtension(bool allowed)
        {
            var dialog = new NewExtensionDialog(this.Module, allowed);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.AddItem(dialog.Item);
        }

        public void Remove()
        {
            var dialog = (IManagementUIService)this.GetService(typeof(IManagementUIService));
            if (
                dialog.ShowMessage("Are you sure that you want to remove the selected file extension?", this.Name,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) !=
                DialogResult.Yes)
            {
                return;
            }

            RemoveItem();
        }

        public override void Load()
        {
            LoadItems();
        }

        protected override ConfigurationElementCollection GetCollection(IConfigurationService service)
        {
            ConfigurationSection requestFilteringSection = service.GetSection("system.webServer/security/requestFiltering");
            return requestFilteringSection.GetCollection("fileExtensions");
        }

        public override bool ShowHelp()
        {
            Process.Start("http://go.microsoft.com/fwlink/?LinkId=210526");
            return true;
        }

        public override string Name
        {
            get
            {
                return "File Name Extensions";
            }
        }
    }
}
