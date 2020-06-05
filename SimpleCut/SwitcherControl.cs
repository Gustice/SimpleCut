using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using BMDSwitcherAPI;
using SimpleCut.Utils;

namespace SimpleCut
{
    public delegate void ReportView(long programId, bool keyAiring);
    public delegate void ReportProgress(int percent);

    public class SwitcherControl
    {
        private IBMDSwitcherDiscovery m_switcherDiscovery;
        private IBMDSwitcher m_switcher;
        private IBMDSwitcherMixEffectBlock m_mixEffectBlock1;
        private IBMDSwitcherKey m_switcherKey;
        private IBMDSwitcherTransitionDVEParameters m_switcherKeyPreset;
        private IBMDSwitcherTransitionParameters m_transitionParam;

        private SwitcherMonitor m_switcherMonitor;
        private MixEffectBlockMonitor m_mixEffectBlockMonitor;

        private bool m_moveSliderDownwards = false;

        private List<InputMonitor> m_inputMonitors = new List<InputMonitor>();

        public SwitcherControl(System.Windows.Threading.Dispatcher dispatcher, ReportView reportProgrammView, ReportProgress progress)
        {
            m_switcherMonitor = new SwitcherMonitor();
            m_switcherMonitor.SwitcherDisconnected += new SwitcherEventHandler((s, a) => dispatcher.Invoke((Action)(() => SwitcherDisconnected())));
            m_mixEffectBlockMonitor = new MixEffectBlockMonitor();
            m_mixEffectBlockMonitor.TransitionPositionChanged += new SwitcherEventHandler((s, a) => dispatcher.Invoke((Action)(() => UpdateSliderPosition())));
            m_mixEffectBlockMonitor.InTransitionChanged += new SwitcherEventHandler((s, a) => dispatcher.Invoke((Action)(() => OnInTransitionChanged())));

            m_switcherDiscovery = new CBMDSwitcherDiscovery();
            if (m_switcherDiscovery == null)
            {
                MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software may not be installed.", "Error");
                Environment.Exit(1);
            }

            SwitcherDisconnected();     // start with switcher disconnected
            _reportProgrammView = reportProgrammView;
            _progress = progress;
        }

        public void ConnectToIp(string ipAddress)
        {
            _BMDSwitcherConnectToFailure failReason = 0;

            try
            {
                m_switcherDiscovery.ConnectTo(ipAddress, out m_switcher, out failReason);
            }
            catch (COMException)
            {
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        MessageBox.Show("Connection failed for unknown reason", "Error");
                        break;
                }
                return;
            }

            SwitcherConnected();
        }

        public string SwitcherName { get; private set; }
        public List<StringObjectPair<long>> ComboBoxItems { get; private set; } = new List<StringObjectPair<long>>();
        public bool IsConnected { get; internal set; }
        public ReportView _reportProgrammView { get; }
        public ReportProgress _progress { get; }

        private void SwitcherConnected()
        {
            string switcherName;
            m_switcher.GetProductName(out switcherName);
            SwitcherName = switcherName;

            // Install SwitcherMonitor callbacks:
            m_switcher.AddCallback(m_switcherMonitor);

            GetInputs();

            m_mixEffectBlock1 = GetMixBox1();
            if (m_mixEffectBlock1 != null)
                m_mixEffectBlock1.AddCallback(m_mixEffectBlockMonitor);
            else
                MessageBox.Show("Unexpected: Could not get first mix effect block", "Error");

            m_switcherKey = GetSwitcherKey1(m_mixEffectBlock1);
            m_switcherKeyPreset = GetKeyParam(m_mixEffectBlock1);
            m_transitionParam = GetKeyTransition(m_mixEffectBlock1);

            UpdatePopupItems();
            UpdateSliderPosition();
            IsConnected = true;

            m_switcherKey.SetOnAir(0);
        }


        void GetInputs()
        {
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            m_switcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
            if (inputIteratorPtr != null)
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }

            if (inputIterator != null)
            {
                IBMDSwitcherInput input;
                inputIterator.Next(out input);
                while (input != null)
                {
                    InputMonitor newInputMonitor = new InputMonitor(input);
                    input.AddCallback(newInputMonitor);
                    newInputMonitor.LongNameChanged += new SwitcherEventHandler(OnInputLongNameChanged);

                    m_inputMonitors.Add(newInputMonitor);

                    inputIterator.Next(out input);
                }
            }
        }

        IBMDSwitcherMixEffectBlock GetMixBox1()
        {
            IBMDSwitcherMixEffectBlockIterator meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
            m_switcher.CreateIterator(ref meIteratorIID, out meIteratorPtr);
            if (meIteratorPtr != null)
            {
                meIterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
            }

            if (meIterator == null)
                return null;

            IBMDSwitcherMixEffectBlock temp = null;

            if (meIterator != null)
            {
                meIterator.Next(out temp);
            }

            return temp;
        }


        IBMDSwitcherKey GetSwitcherKey1(IBMDSwitcherMixEffectBlock block)
        {
            IBMDSwitcherKeyIterator meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherKeyIterator).GUID;
            block.CreateIterator(ref meIteratorIID, out meIteratorPtr);
            if (meIteratorPtr != null)
            {
                meIterator = (IBMDSwitcherKeyIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
            }

            if (meIterator == null)
                return null;

            IBMDSwitcherKey temp = null;

            if (meIterator != null)
            {
                meIterator.Next(out temp);
            }

            return temp;
        }

        IBMDSwitcherTransitionDVEParameters GetKeyParam(IBMDSwitcherMixEffectBlock block)
        {
            IBMDSwitcherTransitionDVEParameters meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherTransitionDVEParameters).GUID;

            IntPtr meIPtr = Marshal.GetIUnknownForObject(block);
            Marshal.QueryInterface(meIPtr, ref meIteratorIID, out meIteratorPtr);

            meIterator = (IBMDSwitcherTransitionDVEParameters)Marshal.GetObjectForIUnknown(meIteratorPtr);

            return meIterator;
        }

        IBMDSwitcherTransitionParameters GetKeyTransition(IBMDSwitcherMixEffectBlock block)
        {
            IBMDSwitcherTransitionParameters meIterator = null;
            IntPtr meIteratorPtr;
            Guid meIteratorIID = typeof(IBMDSwitcherTransitionParameters).GUID;

            IntPtr meIPtr = Marshal.GetIUnknownForObject(block);
            Marshal.QueryInterface(meIPtr, ref meIteratorIID, out meIteratorPtr);

            meIterator = (IBMDSwitcherTransitionParameters)Marshal.GetObjectForIUnknown(meIteratorPtr);

            return meIterator;
        }

        private void SwitcherDisconnected()
        {
            IsConnected = false;

            // Remove all input monitors, remove callbacks
            foreach (InputMonitor inputMon in m_inputMonitors)
            {
                inputMon.Input.RemoveCallback(inputMon);
                inputMon.LongNameChanged -= new SwitcherEventHandler(OnInputLongNameChanged);
            }
            m_inputMonitors.Clear();

            if (m_mixEffectBlock1 != null)
            {
                // Remove callback
                m_mixEffectBlock1.RemoveCallback(m_mixEffectBlockMonitor);

                // Release reference
                m_mixEffectBlock1 = null;
            }

            if (m_switcher != null)
            {
                // Remove callback:
                m_switcher.RemoveCallback(m_switcherMonitor);

                // release reference:
                m_switcher = null;
            }
        }

        private void UpdatePopupItems()
        {
            // Clear the combo boxes:
            ComboBoxItems.Clear();

            // Get an input iterator.
            IBMDSwitcherInputIterator inputIterator = null;
            IntPtr inputIteratorPtr;
            Guid inputIteratorIID = typeof(IBMDSwitcherInputIterator).GUID;
            m_switcher.CreateIterator(ref inputIteratorIID, out inputIteratorPtr);
            if (inputIteratorPtr != null)
            {
                inputIterator = (IBMDSwitcherInputIterator)Marshal.GetObjectForIUnknown(inputIteratorPtr);
            }

            if (inputIterator == null)
                return;

            IBMDSwitcherInput input;
            inputIterator.Next(out input);
            while (input != null)
            {
                string inputName;
                long inputId;

                input.GetInputId(out inputId);
                input.GetLongName(out inputName);

                // Add items to list:
                ComboBoxItems.Add(new StringObjectPair<long>(inputName, inputId));
                inputIterator.Next(out input);
            }

        }

        private void OnInputLongNameChanged(object sender, object args)
        {
            // ToDo ComboBox Items must be updated
            //this.Invoke((Action)(() => UpdatePopupItems()));
        }

        private void UpdateSliderPosition()
        {
            double transitionPos;

            m_mixEffectBlock1.GetTransitionPosition(out transitionPos);
            _progress?.Invoke((int)transitionPos * 100);
        }

        private void OnInTransitionChanged()
        {
            int inTransition;

            m_mixEffectBlock1.GetInTransition(out inTransition);

            if (inTransition == 0)
            {
                long programId;
                int keyOnnAir;

                m_mixEffectBlock1.GetProgramInput(out programId);
                m_switcherKey.GetOnAir(out keyOnnAir);

                _reportProgrammView?.Invoke(programId, keyOnnAir == 1 ? true : false);

                //long previewId;
                //m_mixEffectBlock1.GetPreviewInput(out previewId);
            }
        }

        public void BlankOut(bool switchFast = false)
        {
            if (m_mixEffectBlock1 != null)
            {
                m_mixEffectBlock1.SetFadeToBlackRate(25);
                m_mixEffectBlock1.PerformFadeToBlack();
            }
        }

        public void SwitchView(long inputId, bool switchFast = false)
        {
            if (m_mixEffectBlock1 != null)
                m_mixEffectBlock1.SetPreviewInput(inputId);


            if (m_mixEffectBlock1 != null)
            {
                if (switchFast == false)
                    m_mixEffectBlock1.PerformAutoTransition();
                else
                    m_mixEffectBlock1.PerformCut();
            }
        }

        public void PresetKey(bool backbround, bool key, bool isTransparent = false)
        {
            if (isTransparent)
                m_switcherKey.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeLuma);
            else
                m_switcherKey.SetType(_BMDSwitcherKeyType.bmdSwitcherKeyTypeChroma);
            
                        
            if (backbround && key)
            {
                m_transitionParam.SetNextTransitionSelection(
                    _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground |
                    _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);
            }
            else if (key)
            {
                m_transitionParam.SetNextTransitionSelection(
                    _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionKey1);
            }
            else
            {
                m_transitionParam.SetNextTransitionSelection(
                    _BMDSwitcherTransitionSelection.bmdSwitcherTransitionSelectionBackground);
            }
        }
    }
}
