using Bot.Config;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace Bot
{
    public delegate void SetPropertyDelegate<TCtl, TProp>(TCtl control, Expression<Func<TCtl, TProp>> propexpr, TProp value) where TCtl : Control;
    public delegate TProp GetPropertyDelegate<TCtl, TProp>(TCtl control, Expression<Func<TProp>> propexpr) where TCtl : Control;
    public delegate void InvokeActionDelegate<TCtl>(TCtl control, Delegate dlg, params object[] args) where TCtl : Control;
    public delegate TResult InvokeFuncDelegate<TCtl, TResult>(TCtl control, Delegate dlg, params object[] args) where TCtl : Control;

    public static class Extensions
    {
        public static void SetProperty<TCtl, TProp>(this TCtl control, Expression<Func<TCtl, TProp>> propexpr, TProp value) where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (propexpr == null)
                throw new ArgumentNullException(nameof(propexpr));
            if (control.InvokeRequired)
            {
                control.Invoke(new SetPropertyDelegate<TCtl, TProp>(SetProperty), control, propexpr, value);
                return;
            }
            var propexprm = propexpr.Body as MemberExpression;
            if (propexprm == null)
                throw new ArgumentException("Invalid member expression.", nameof(propexpr));
            var prop = propexprm.Member as PropertyInfo;
            if (prop == null)
                throw new ArgumentException("Invalid property supplied.", nameof(propexpr));
            prop.SetValue(control, value);
        }

        public static TProp GetProperty<TCtl, TProp>(this TCtl control, Expression<Func<TProp>> propexpr) where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (propexpr == null)
                throw new ArgumentNullException(nameof(propexpr));
            if (control.InvokeRequired)
                return (TProp)control.Invoke(new GetPropertyDelegate<TCtl, TProp>(GetProperty), control, propexpr);
            var propexprm = propexpr.Body as MemberExpression;
            if (propexprm == null)
                throw new ArgumentException("Invalid member expression.", nameof(propexpr));
            var prop = propexprm.Member as PropertyInfo;
            if (prop == null)
                throw new ArgumentException("Invalid property supplied.", nameof(propexpr));
            return (TProp)prop.GetValue(control);
        }

        public static void InvokeAction<TCtl>(this TCtl control, Delegate dlg, params object[] args) where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (dlg == null)
                throw new ArgumentNullException(nameof(dlg));
            if (control.InvokeRequired)
            {
                control.Invoke(new InvokeActionDelegate<TCtl>(InvokeAction), control, dlg, args);
                return;
            }
            dlg.DynamicInvoke(args);
        }
        
        public static TResult InvokeFunc<TCtl, TResult>(this TCtl control, Delegate dlg, params object[] args) where TCtl : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (dlg == null)
                throw new ArgumentNullException(nameof(dlg));
            return control.InvokeRequired
                ? (TResult)control.Invoke(new InvokeFuncDelegate<TCtl, TResult>(InvokeFunc<TCtl, TResult>), control, dlg, args)
                : (TResult)dlg.DynamicInvoke(args);
        }

        public static bool getEvaluatedNSFW(this DiscordChannel Channel) => Channel.IsNSFW || Config.ChCfgMgr.getCh(Channel.Id).Nsfw;

        public static T get<G, T>(this Dictionary<G, T> dict, G key, T def)
        {
            if (!dict.ContainsKey(key))
                dict[key] = def;
            return dict[key];
        }

        public static T set<G, T>(this Dictionary<G, T> dict, G key, T val)
        {
            dict[key] = val;
            return dict[key];
        }

        public static bool tryCast<T>(this object o, out T parsed)
        {
            try
            {
                parsed = (T)o;
                return true;
            }
            catch
            {
                parsed = default(T);
                return false;
            }
        }

        public static G mod<T, G>(this T self, Func<T, G> func) => func.Invoke(self);
        public static void modCh(this ulong ID, Func<ChannelConfig, ChannelConfig> func) => ChCfgMgr.setCh(ID, ChCfgMgr.getCh(ID).mod(func));
    }
}
