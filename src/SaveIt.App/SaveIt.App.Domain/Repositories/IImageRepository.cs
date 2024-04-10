using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IImageRepository
{
    Task CreateImageAsync(ImageEntity image);
}
