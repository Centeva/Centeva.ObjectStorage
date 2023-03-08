namespace Centeva.ObjectStorage.UnitTests.Fixtures
{
    internal class TestProvider : IObjectStorage
    {
        public Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string objectName, Stream dataStream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool SupportsSignedUrls => true;

        public Task<Uri> GetSignedUrlAsync(string objectName, TimeSpan lifetime, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
