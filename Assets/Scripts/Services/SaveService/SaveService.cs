using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dutpekmezi.Services.SaveServices
{
    public class SaveService : ISaveService
    {
        private readonly ISaveHandler _saveHandler;
        private readonly Dictionary<Type, object> _repositories = new();
        private readonly PrimitiveSaveHelper _primitiveHelper;
        public PrimitiveSaveHelper Raw => _primitiveHelper;

        public SaveService(ISaveHandler saveHandler)
        {
            this._saveHandler = saveHandler;
            this._primitiveHelper = new PrimitiveSaveHelper(saveHandler);
        }

        public void Register<T>(string key) where T : ISaveable, new()
        {
            if (_repositories.ContainsKey(typeof(T)))
                return;

            var repo = new SaveRepository<T>(_saveHandler, key);
            _repositories.Add(typeof(T), repo);
        }

        public SaveRepository<T> GetRepository<T>() where T : ISaveable, new()
        {
            if (_repositories.TryGetValue(typeof(T), out var repo))
                return repo as SaveRepository<T>;

            return null;
        }
    }
}
