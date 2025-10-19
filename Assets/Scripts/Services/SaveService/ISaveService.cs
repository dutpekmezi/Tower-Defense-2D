using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resolve.Services.SaveServices
{
    public interface ISaveService
    {
        public PrimitiveSaveHelper Raw {  get; }

        void Register<T>(string key) where T : ISaveable, new();

        SaveRepository<T> GetRepository<T>() where T : ISaveable, new();
    }
}