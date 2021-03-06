﻿using DotNetNuke.Collections.Internal;
using DotNetNuke.Framework.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.FileIndexer
{
    public class FileIndexerManager
    {

        private static readonly ILogAdapter Logger = AppConfig.Instance.LogAdapter.GetLogAdapter(typeof(FileIndexerManager));
        private static NaiveLockingList<IFileIndexer> _fileIndexers;

        public static void RegisterFileIndexers()
        {
            _fileIndexers = new NaiveLockingList<IFileIndexer>();

            foreach (IFileIndexer fi in GetFileIndexers())
            {
                _fileIndexers.Add(fi);
            }
        }

        private static IEnumerable<IFileIndexer> GetFileIndexers()
        {
            var typeLocator = new TypeLocator();
            IEnumerable<Type> types = typeLocator.GetAllMatchingTypes(IsValidDataSourceProvider);

            foreach (Type filterType in types)
            {
                IFileIndexer filter;
                try
                {
                    filter = Activator.CreateInstance(filterType) as IFileIndexer;
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to create {filterType.FullName} while GetFileIndexers. {e.Message}");
                    filter = null;
                }

                if (filter != null)
                {
                    yield return filter;
                }
            }
        }

        private static bool IsValidDataSourceProvider(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IFileIndexer).IsAssignableFrom(t);
        }

        public static IFileIndexer GetFileIndexer(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;

            var fileIndexer = _fileIndexers.SingleOrDefault(ds => ds.CanIndex(file));
            return fileIndexer;
        }
    }

}