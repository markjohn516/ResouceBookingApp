using ResourceBooking.Api.Repositories;
using ResourceBooking.Core.DTOs;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Services;

public interface IResourceService
{
    Task<ResourceDto?> GetResourceAsync(int id);
    Task<List<ResourceDto>> GetAllResourcesAsync();
    Task<List<ResourceDto>> GetActiveResourcesAsync();
    Task<ResourceDto> CreateResourceAsync(CreateResourceDto createDto);
    Task UpdateResourceAsync(int id, UpdateResourceDto updateDto);
    Task DisableResourceAsync(int id);
    Task EnableResourceAsync(int id);
}

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceService(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<ResourceDto?> GetResourceAsync(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        return resource == null ? null : MapToDto(resource);
    }

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        var resources = await _resourceRepository.GetAllAsync();
        return resources.Select(MapToDto).ToList();
    }

    public async Task<List<ResourceDto>> GetActiveResourcesAsync()
    {
        var resources = await _resourceRepository.GetAllActiveAsync();
        return resources.Select(MapToDto).ToList();
    }

    public async Task<ResourceDto> CreateResourceAsync(CreateResourceDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Resource name is required.");

        if (string.IsNullOrWhiteSpace(createDto.Type))
            throw new ArgumentException("Resource type is required.");

        var resource = new Resource
        {
            Name = createDto.Name.Trim(),
            Type = createDto.Type.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _resourceRepository.AddAsync(resource);
        return MapToDto(resource);
    }

    public async Task UpdateResourceAsync(int id, UpdateResourceDto updateDto)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null)
            throw new InvalidOperationException($"Resource with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Resource name is required.");

        if (string.IsNullOrWhiteSpace(updateDto.Type))
            throw new ArgumentException("Resource type is required.");

        resource.Name = updateDto.Name.Trim();
        resource.Type = updateDto.Type.Trim();
        resource.IsActive = updateDto.IsActive;

        await _resourceRepository.UpdateAsync(resource);
    }

    public async Task DisableResourceAsync(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null)
            throw new InvalidOperationException($"Resource with ID {id} not found.");

        resource.IsActive = false;
        await _resourceRepository.UpdateAsync(resource);
    }

    public async Task EnableResourceAsync(int id)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null)
            throw new InvalidOperationException($"Resource with ID {id} not found.");

        resource.IsActive = true;
        await _resourceRepository.UpdateAsync(resource);
    }

    private static ResourceDto MapToDto(Resource resource)
    {
        return new ResourceDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt
        };
    }
}
