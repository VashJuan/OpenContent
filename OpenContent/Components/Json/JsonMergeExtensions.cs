﻿using Newtonsoft.Json.Linq;
namespace Satrabel.OpenContent.Components.Json
{
    /// <summary>
    /// <para>Extensions for merging JTokens</para>
    /// </summary>
    public static class JsonMergeExtensions
    {
        /// <summary>
        /// <para>Creates a new token which is the merge of the passed tokens</para>
        /// </summary>
        /// <param name="left">Token</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <param name="options">Options for merge</param>
        /// <returns>A new merged token</returns>
        public static JToken JsonMerge(
            this JToken left, JToken right, JsonMergeOptions options)
        {
            if (left.Type != JTokenType.Object)
                return right.DeepClone();

            var leftClone = (JContainer)left.DeepClone();
            JsonMergeInto(leftClone, right, options);

            return leftClone;
        }

        /// <summary>
        /// <para>Creates a new token which is the merge of the passed tokens</para>
        /// <para>Default options are used</para>
        /// </summary>
        /// <param name="left">Token</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <returns>A new merged token</returns>
        public static JToken JsonMerge(this JToken left, JToken right)
        {
            return JsonMerge(left, right, JsonMergeOptions.Default);
        }

        /// <summary>
        /// <para>Merge the right token into the left</para>
        /// </summary>
        /// <param name="left">Token to be merged into</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <param name="options">Options for merge</param>
        public static void JsonMergeInto(
            this JContainer left, JToken right, JsonMergeOptions options)
        {
            foreach (var rightChild in right.Children<JProperty>())
            {
                var rightChildProperty = rightChild;
                var leftPropertyValue = left.SelectToken(rightChildProperty.Name);

                if (leftPropertyValue == null)
                {
                    // no matching property, just add 
                    left.Add(rightChild);
                }
                else
                {
                    var leftProperty = (JProperty)leftPropertyValue.Parent;

                    var leftArray = leftPropertyValue as JArray;
                    var rightArray = rightChildProperty.Value as JArray;
                    if (leftArray != null && rightArray != null)
                    {
                        switch (options.ArrayHandling)
                        {
                            case JsonMergeOptionArrayHandling.Concat:

                                foreach (var rightValue in rightArray)
                                {
                                    leftArray.Add(rightValue);
                                }

                                break;
                            case JsonMergeOptionArrayHandling.Overwrite:

                                leftProperty.Value = rightChildProperty.Value;
                                break;
                        }
                    }

                    else
                    {
                        var leftObject = leftPropertyValue as JObject;
                        if (leftObject == null)
                        {
                            // replace value
                            leftProperty.Value = rightChildProperty.Value;
                        }

                        else
                            // recurse object
                            JsonMergeInto(leftObject, rightChildProperty.Value, options);
                    }
                }
            }
        }

        /// <summary>
        /// <para>Merge the right token into the left</para>
        /// <para>Default options are used</para>
        /// </summary>
        /// <param name="left">Token to be merged into</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        public static void JsonMergeInto(
            this JContainer left, JToken right)
        {
            JsonMergeInto(left, right, JsonMergeOptions.Default);
        }
    }
}