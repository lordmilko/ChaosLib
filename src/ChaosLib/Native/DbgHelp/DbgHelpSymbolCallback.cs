using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ClrDebug.DbgEng;

namespace ChaosLib
{
    public unsafe class DbgHelpSymbolCallback
    {
        public delegate bool ReadMemoryDelegate(IMAGEHLP_CBA_READ_MEMORY* data);

        public delegate bool DeferredSymbolLoadDelegate(IMAGEHLP_DEFERRED_SYMBOL_LOAD64* data);

        #region Xml

        public enum XmlEventKind
        {
            ActivityStart,
            Log,
            ActivityEnd,
            Progress
        }

        public abstract class XmlEvent
        {
            public XmlEventKind Kind { get; }

            protected XmlEvent(XmlEventKind kind)
            {
                Kind = kind;
            }
        }

        [DebuggerDisplay("Name = {Name}, Details = {Details}")]
        public class XmlActivityStartEvent : XmlEvent
        {
            public string Name { get; }
            public string Details { get; }

            public XmlActivityStartEvent(string name, string details) : base(XmlEventKind.ActivityStart)
            {
                Name = name;
                Details = details;
            }
        }

        [DebuggerDisplay("Message = {Message}, Component = {Component}, Category = {Category}, Level = {Level}")]
        public class XmlLogEvent : XmlEvent
        {
            public string Message { get; }
            public string Component { get; }
            public string Category { get; }
            public string Level { get; }

            public XmlLogEvent(string message, string component, string category, string level) : base(XmlEventKind.Log)
            {
                Message = message;
                Component = component;
                Category = category;
                Level = level;
            }
        }

        [DebuggerDisplay("End")]
        public class XmlActivityEndEvent : XmlEvent
        {
            public XmlActivityEndEvent() : base(XmlEventKind.ActivityEnd)
            {
            }
        }

        [DebuggerDisplay("Percent = {Percent}")]
        public class XmlProgressEvent : XmlEvent
        {
            public int Percent { get; }

            public XmlProgressEvent(int percent) : base(XmlEventKind.Progress)
            {
                Percent = percent;
            }
        }

        #endregion

        //All of these may need to return true, even if their documentation doesn't say so
        public DeferredSymbolLoadDelegate OnDeferredSymbolLoadStart;
        public DeferredSymbolLoadDelegate OnDeferredSymbolLoadPartial;
        public DeferredSymbolLoadDelegate OnDeferredSymbolLoadComplete;
        public DeferredSymbolLoadDelegate OnDeferredSymbolLoadFailure;
        public DeferredSymbolLoadDelegate OnDeferredSymbolLoadCancel;
        public ReadMemoryDelegate OnReadMemory;

        public event EventHandler<XmlEvent> OnXml;

        internal unsafe bool CallbackHandler(
            IntPtr hProcess,
            CBA ActionCode,
            long CallbackData,
            long UserContext)
        {
            switch (ActionCode)
            {
                case CBA.CBA_DEFERRED_SYMBOL_LOAD_START:
                    if (OnDeferredSymbolLoadStart != null)
                        return OnDeferredSymbolLoadStart((IMAGEHLP_DEFERRED_SYMBOL_LOAD64*) CallbackData);
                    break;

                case CBA.CBA_DEFERRED_SYMBOL_LOAD_PARTIAL:
                    if (OnDeferredSymbolLoadPartial != null)
                        return OnDeferredSymbolLoadPartial((IMAGEHLP_DEFERRED_SYMBOL_LOAD64*) CallbackData);
                    break;

                case CBA.CBA_DEFERRED_SYMBOL_LOAD_COMPLETE:
                    if (OnDeferredSymbolLoadComplete != null)
                        return OnDeferredSymbolLoadComplete((IMAGEHLP_DEFERRED_SYMBOL_LOAD64*) CallbackData);
                    break;

                case CBA.CBA_DEFERRED_SYMBOL_LOAD_FAILURE:
                    if (OnDeferredSymbolLoadFailure != null)
                        return OnDeferredSymbolLoadFailure((IMAGEHLP_DEFERRED_SYMBOL_LOAD64*) CallbackData);
                    break;

                case CBA.CBA_DEFERRED_SYMBOL_LOAD_CANCEL:
                    if (OnDeferredSymbolLoadCancel != null)
                        return OnDeferredSymbolLoadCancel((IMAGEHLP_DEFERRED_SYMBOL_LOAD64*) CallbackData);
                    break;

                case CBA.CBA_SYMBOLS_UNLOADED:
                case CBA.CBA_DUPLICATE_SYMBOL:
                    throw new NotImplementedException($"Don't know how to handle {nameof(CBA)} '{ActionCode}'.");

                case CBA.CBA_READ_MEMORY:
                    if (OnReadMemory != null)
                        return OnReadMemory((IMAGEHLP_CBA_READ_MEMORY*)CallbackData);
                    break;

                case CBA.CBA_SET_OPTIONS:
                    var options = *(SYMOPT*) CallbackData;
                    throw new NotImplementedException($"Don't know how to handle {nameof(CBA)} '{ActionCode}'.");

                case CBA.CBA_EVENT:
                case CBA.CBA_DEBUG_INFO:
                case CBA.CBA_SRCSRV_INFO:
                case CBA.CBA_SRCSRV_EVENT:
                case CBA.CBA_UPDATE_STATUS_BAR:
                case CBA.CBA_ENGINE_PRESENT:
                    throw new NotImplementedException($"Don't know how to handle {nameof(CBA)} '{ActionCode}'.");

                case CBA.CBA_CHECK_ENGOPT_DISALLOW_NETWORK_PATHS: //Seems like a special DbgEng specific event. We don't care about this; ignore
                    break;

                case CBA.CBA_CHECK_ARM_MACHINE_THUMB_TYPE_OVERRIDE:
                    throw new NotImplementedException($"Don't know how to handle {nameof(CBA)} '{ActionCode}'.");

                case CBA.CBA_XML_LOG:
                    ParseXmlLog(CallbackData);
                    break;

                case CBA.CBA_MAP_JIT_SYMBOL: //Seems to relate to doing function table lookups of CLR modules. We don't know what the callback data is; ignore
                    break;

                default:
                    throw new NotImplementedException($"Don't know how to handle {nameof(CBA)} '{ActionCode}'.");
            }

            //You want to return false by default, not true. dbghelp!modloadWorker will return ERROR_CANCELLED when this method returns true but there was in fact
            //some type of DIA error or something
            return false;
        }

        private void ParseXmlLog(long callbackData)
        {
            var str = Marshal.PtrToStringUni((IntPtr) callbackData);

            //We can't use XmlReader; it will get upset that the XML tag is not closed as well.
            //As such, it seems we have no choice but to use regular expressions

            var match = Regex.Match(str, "<Activity name=\"(.+?)\" details=\"(.+?)\">");

            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var details = match.Groups[2].Value;

                OnXml?.Invoke(this, new XmlActivityStartEvent(name, details));
            }
            else
            {
                match = Regex.Match(str, "<Log message=\"(.+?)\" component=\"(.+?)\" category=\"(.+?)\" level=\"(.+?)\"/>");

                if (match.Success)
                {
                    var message = match.Groups[1].Value;
                    var component = match.Groups[2].Value;
                    var category = match.Groups[3].Value;
                    var level = match.Groups[4].Value;

                    OnXml?.Invoke(this, new XmlLogEvent(message, component, category, level));
                }
                else
                {
                    if (str == "</Activity>\n")
                    {
                        OnXml?.Invoke(this, new XmlActivityEndEvent());
                    }
                    else
                    {
                        match = Regex.Match(str, "<Progress percent=\"(.+?)\"/>");

                        if (match.Success)
                        {
                            var percent = Convert.ToInt32(match.Groups[1].Value);
                            OnXml?.Invoke(this, new XmlProgressEvent(percent));
                        }
                        else
                            Debug.Assert(false, $"Don't know how to handle XML '{str}'");
                    }
                }
            }
        }
    }
}
