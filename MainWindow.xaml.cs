using System.Windows;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace VirtualBoxTimeSyncChanger
{
    public partial class MainWindow : Window
    {
        public string machineDefinition = "";
        public string machineName = "";
        public string machineUUID = "";
        public string machineType = "";
        //Global language variables
        public string syncstate_disabled = "is disabled";
        public string syncstate_enabled = "is enabled";
        public string syncstate_novboxmanage = "Can't find VBoxManage.exe";
        public MainWindow()
        {
            InitializeComponent();
            setLang(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }
        public void setLang(string lang)
        {
            if(lang=="pl")
            {
                btnOpenFile.Content = "Wybierz plik .vbox wirtualnej maszyny";
                btnSyncDisable.Content = "Wyłącz";
                btnSyncEnable.Content = "Włącz";
                GUI_Label_machineName.Content = "Nazwa maszyny";
                GUI_Label_machineUUID.Content = "UUID maszyny";
                GUI_Label_machineOSType.Content = "Typ maszyny";
                GUI_Label_SyncState.Content = "Synchronizacja";
                //Global variables
                syncstate_disabled = "jest wyłączona";
                syncstate_enabled = "jest włączona";
                syncstate_novboxmanage = "Nie znaleziono VBoxManage.exe";
            }
        }
        public void UpdateMachineInfo()
        {
            machineName = getValue("name");
            machineUUID = getValue("uuid");
            machineType = getValue("OSType");
            GUI_machineName.Content = machineName;
            GUI_UUID.Content = machineUUID;
            GUI_machineOSType.Content = machineType;
            GUI_MachineData.Visibility = Visibility.Visible;
            btnSyncDisable.Visibility = Visibility.Visible;
            btnSyncEnable.Visibility = Visibility.Visible;
            GUI_Label_SyncState.Visibility = Visibility.Visible;
            UpdateSyncState();
            GUI_SyncState.Visibility = Visibility.Visible;
        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                machineDefinition = File.ReadAllText(openFileDialog.FileName);
                UpdateMachineInfo();
            }
        }
        public string getValue(string value)
        {
            string temp = machineDefinition.Substring(machineDefinition.IndexOf(value+"="));
            temp = temp.Substring(temp.IndexOf('"')+1);
            temp = temp.Remove(temp.IndexOf('"'));
            return temp;
            
        }
        public void UpdateSyncState()
        {
            string searchFor = "<ExtraDataItem name=\"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" value=\"";
            string temp = "";
            if (machineDefinition.Contains(searchFor))
            {
                temp = machineDefinition.Substring(machineDefinition.IndexOf(searchFor), searchFor.Length + 1);
                if (temp.EndsWith("0"))
                    temp = "0";
                else
                    temp = "1";
            }
            else
                temp = "0";
            if (temp == "0")
                GUI_SyncState.Content = syncstate_enabled;
            else if (temp == "1")
                GUI_SyncState.Content = syncstate_disabled;
        }
        private void btnSyncDisable_Click(object sender, RoutedEventArgs e)
        {
            Process vboxmanage = null;
            try
            {
                vboxmanage = Process.Start("VBoxManage.exe", "setextradata " + machineUUID + " \"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" 1");
            }
            catch(System.ComponentModel.Win32Exception exc)
            {
                GUI_SyncState.Content = syncstate_novboxmanage;
            }
            if (GUI_SyncState.Content != syncstate_novboxmanage)
            {
                while (vboxmanage.HasExited == false) {/*wait for vboxmanage to close because if something terminate vboxmanage before it change value this program will show not real state of time sync*/}
                GUI_SyncState.Content = syncstate_disabled;
            }
        }

        private void btnSyncEnable_Click(object sender, RoutedEventArgs e)
        {
            Process vboxmanage = null;
            try
            {
            vboxmanage = Process.Start("VBoxManage.exe", "setextradata " + machineUUID + " \"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" 0");
            }
            catch (System.ComponentModel.Win32Exception exc)
            {
                GUI_SyncState.Content = syncstate_novboxmanage;
            }
            if (GUI_SyncState.Content != syncstate_novboxmanage)
            {
                while (vboxmanage.HasExited == false) {/*wait for vboxmanage to close because if something terminate vboxmanage before it change value this program will show not real state of time sync*/}
                GUI_SyncState.Content = syncstate_enabled;
            }
        }
    }
}
