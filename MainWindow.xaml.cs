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
        public string changeAllMachinesValue_Status_enabled = "All virtual machines have time synchronization enabled";
        public string changeAllMachinesValue_Status_disabled = "All virtual machines have time synchronization disabled";
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
                btnPopup.Content = "Włącz/wyłącz synchronizację na wszystkich maszynach";
                GUI_Popup_changeAllMachinesValue_Label.Content = "Co chcesz zrobić z wszystkimi wirtualnymi maszynami?";
                GUI_Popup_changeAllMachinesValue_btnEnable.Content = "Włącz";
                GUI_Popup_changeAllMachinesValue_btnNothing.Content = "Nic";
                GUI_Popup_changeAllMachinesValue_btnDisable.Content = "Wyłącz";
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
                changeAllMachinesValue_Status_enabled = "Wszystkie maszyny mają włączoną sychronizację czasu";
                changeAllMachinesValue_Status_disabled = "Wszystkie maszyny mają wyłączoną sychronizację czasu";
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
            GUI_changeAllMachinesValue_Status.Visibility = Visibility.Collapsed;
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
            Process vboxmanage = new Process();
            vboxmanage.StartInfo.FileName = "VBoxManage.exe";
            vboxmanage.StartInfo.Arguments = "setextradata " + machineUUID + " \"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" 1";
            vboxmanage.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                vboxmanage.Start();
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
            Process vboxmanage = new Process();
            vboxmanage.StartInfo.FileName = "VBoxManage.exe";
            vboxmanage.StartInfo.Arguments = "setextradata " + machineUUID + " \"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" 0";
            vboxmanage.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                vboxmanage.Start();
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

        private void btnPopup_Click(object sender, RoutedEventArgs e)
        {
            GUI_Popup_ChangeAllMachinesValue.IsOpen = true;
            GUI_MachineData.Visibility = Visibility.Hidden;
            btnSyncDisable.Visibility = Visibility.Hidden;
            btnSyncEnable.Visibility = Visibility.Hidden;
            GUI_Label_SyncState.Visibility = Visibility.Hidden;
            UpdateSyncState();
            GUI_SyncState.Visibility = Visibility.Hidden;
        }
        private void GUI_Popup_changeAllMachinesValue_btnEnable_Click(object sender, RoutedEventArgs e)
        {
            GUI_Popup_ChangeAllMachinesValue.IsOpen = false;
            Process vboxmanage = new Process();
            string output = null;
            vboxmanage.StartInfo.FileName = "VBoxManage.exe";
            vboxmanage.StartInfo.Arguments = "list vms";
            vboxmanage.StartInfo.UseShellExecute = false;
            vboxmanage.StartInfo.RedirectStandardOutput = true;
            vboxmanage.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            vboxmanage.StartInfo.CreateNoWindow = true;
            try
            {
                vboxmanage.Start();
                output = vboxmanage.StandardOutput.ReadToEnd();
            }
            catch (System.ComponentModel.Win32Exception exc)
            {
                GUI_changeAllMachinesValue_Status.Content = syncstate_novboxmanage;
            }
            if (GUI_changeAllMachinesValue_Status.Content != syncstate_novboxmanage)
            {
                while (vboxmanage.HasExited == false) {/*wait for vboxmanage to close because if something terminate vboxmanage before it change value this program will not show real state of time sync*/}
            }
            string[,] vmList = VMListToArray(output);
            MultiChangeMachineValue(vmList,0);
            GUI_changeAllMachinesValue_Status.Content = changeAllMachinesValue_Status_enabled;
            GUI_changeAllMachinesValue_Status.Visibility = Visibility.Visible;
        }
        private void GUI_Popup_changeAllMachinesValue_btnNothing_Click(object sender, RoutedEventArgs e)
        {
            GUI_Popup_ChangeAllMachinesValue.IsOpen = false;
        }
        private void GUI_Popup_changeAllMachinesValue_btnDisable_Click(object sender, RoutedEventArgs e)
        {
            GUI_Popup_ChangeAllMachinesValue.IsOpen = false;
            Process vboxmanage = new Process();
            string output = null;
            vboxmanage.StartInfo.FileName = "VBoxManage.exe";
            vboxmanage.StartInfo.Arguments = "list vms";
            vboxmanage.StartInfo.UseShellExecute = false;
            vboxmanage.StartInfo.RedirectStandardOutput = true;
            vboxmanage.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            vboxmanage.StartInfo.CreateNoWindow = true;
            try
            {
                vboxmanage.Start();
                output = vboxmanage.StandardOutput.ReadToEnd();
            }
            catch (System.ComponentModel.Win32Exception exc)
            {
                GUI_changeAllMachinesValue_Status.Content = syncstate_novboxmanage;
            }
            if (GUI_SyncState.Content != syncstate_novboxmanage)
            {
                while (vboxmanage.HasExited == false) {/*wait for vboxmanage to close because if something terminate vboxmanage before it change value this program will not show real state of time sync*/}
            }
            string[,] vmList = VMListToArray(output);
            MultiChangeMachineValue(vmList, 1);
            GUI_changeAllMachinesValue_Status.Content = changeAllMachinesValue_Status_disabled;
            GUI_changeAllMachinesValue_Status.Visibility = Visibility.Visible;
        }
        public string[,] VMListToArray(string output)
        {
            string[] splitter = new string[] { "\r\n" };
            string[] temp = output.Split(splitter,System.StringSplitOptions.RemoveEmptyEntries);
            string[,] vmList = new string[temp.Length, 2];
            for(int i=0;i<temp.Length;i++)
            {
                vmList[i, 0] = temp[i].Substring(1).Substring(0, temp[i].LastIndexOf('"')-1);
                vmList[i, 1] = temp[i].Substring(temp[i].IndexOf('{'));
            }
            return vmList;
        }
        public void MultiChangeMachineValue(string[,] vmList, int val)
        {
            for (int i = 0; i < vmList.GetLength(0); i++)
            {
                Process vboxmanage = new Process();
                vboxmanage.StartInfo.FileName = "VBoxManage.exe";
                vboxmanage.StartInfo.Arguments = "setextradata " + vmList[i, 1] + " \"VBoxInternal/Devices/VMMDev/0/Config/GetHostTimeDisabled\" "+val;
                vboxmanage.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    vboxmanage.Start();
                }
                catch (System.ComponentModel.Win32Exception exc)
                {
                    GUI_changeAllMachinesValue_Status.Content = syncstate_novboxmanage;
                }
                if (GUI_changeAllMachinesValue_Status.Content != syncstate_novboxmanage)
                {
                    while (vboxmanage.HasExited == false) {/*wait for vboxmanage to close because if something terminate vboxmanage before it change value this program will not show real state of time sync*/}
                }
            }
        }
    }
}
