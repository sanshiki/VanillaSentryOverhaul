using System;
using System.Collections.Generic;

namespace SummonerExpansionMod.ModUtils
{
    public interface IDynamicParam
    {
        public string name { get; set; }
        public float value { get; set; }
        public float upper_limit { get; set; }
        public float lower_limit { get; set; }
    }

    public class DynamicParam : IDynamicParam
    {
        public string name { get; set; }
        public float value { get; set; }
        public float upper_limit { get; set; }
        public float lower_limit { get; set; }
        public Action<float> onChange;

        public DynamicParam(string n, float val, float lower_limit_, float upper_limit_, Action<float> onChange = null)
        {
            name = n;
            value = val;
            upper_limit = upper_limit_;
            lower_limit = lower_limit_;
            onChange = onChange;
        }
    }

    public static class DynamicParamManager
    {
        private static Dictionary<string, IDynamicParam> dynamicParams = new Dictionary<string, IDynamicParam>();

        public static void Register(string name, float value, float lower_limit, float upper_limit, Action<float> onChange = null)
        {
            if(!dynamicParams.ContainsKey(name))
            {
                dynamicParams[name] = new DynamicParam(name, value, lower_limit, upper_limit, onChange);
            }
        }

        public static IDynamicParam Get(string name)
        {
            return dynamicParams.TryGetValue(name, out var param) ? param : null;
        }

        public static IDynamicParam QuickGet(string name, float value, float lower_limit, float upper_limit, Action<float> onChange = null)
        {
            Register(name, value, lower_limit, upper_limit, onChange);
            return Get(name);
        }

        public static IEnumerable<IDynamicParam> GetAll() => dynamicParams.Values;
    }
}