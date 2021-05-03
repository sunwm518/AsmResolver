using System.Collections.Generic;

namespace AsmResolver.IO
{
    public class MemoryMappedFileService : IFileService
    {
        private readonly Dictionary<string, MemoryMappedInputFile> _files = new();

        /// <inheritdoc />
        public IEnumerable<string> GetOpenedFiles() => _files.Keys;

        /// <inheritdoc />
        public IInputFile OpenFile(string filePath)
        {
            if (!_files.TryGetValue(filePath, out var file))
            {
                file = new MemoryMappedInputFile(filePath);
                _files.Add(filePath, file);
            }

            return file;
        }

        /// <inheritdoc />
        public void InvalidateFile(string filePath)
        {
            if (_files.TryGetValue(filePath, out var file))
            {
                file.Dispose();
                _files.Remove(filePath);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var file in _files.Values)
                file.Dispose();
            _files.Clear();
        }
    }
}
