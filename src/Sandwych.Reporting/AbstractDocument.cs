using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Reporting
{

    public abstract class AbstractDocument<TDocument> : ITypedDocument<TDocument>
        where TDocument : AbstractDocument<TDocument>, new()
    {
        public bool IsNew { get; protected set; } = true;
        public abstract Task LoadAsync(Stream inStream, CancellationToken ct = default);
        public abstract Task SaveAsync(Stream outStream, CancellationToken ct = default);

        protected virtual void OnLoaded()
        {
            this.IsNew = false;
        }

        public static async Task<TDocument> LoadFromAsync(Stream inStream, CancellationToken ct = default)
        {
            var doc = new TDocument();
            await doc.LoadAsync(inStream, ct);
            return doc;
        }

        public static async Task<TDocument> LoadFromAsync(string filePath, CancellationToken ct = default)
        {
            using var inStream = File.OpenRead(filePath);
            var doc = new TDocument();
            await doc.LoadAsync(inStream, ct);
            return doc;
        }

        public virtual async Task<TDocument> DuplicateAsync(CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            await this.SaveAsync(ms);
            ms.Position = 0;
            var newDoc = new TDocument();
            await newDoc.LoadAsync(ms);
            return newDoc;
        }

    }

}
