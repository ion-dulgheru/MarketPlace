namespace App.Application.UseCases.Adverts.CreateAdvert;

public class CreateAdvertHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork) 
    : ICommandHandler<CreateAdvertCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAdvertCommand command, CancellationToken ct)
    {
        // 1. Create the domain entity using its constructor
        var advert = new Advert(
            command.UserUuid,
            command.Request.Title,
            command.Request.Description,
            command.Request.Price,
            command.Request.SurfaceArea,
            command.Request.Rooms,
            command.Request.Floor,
            command.Request.Type,
            DateTime.UtcNow.AddDays(30) // Set expiration date
        );

        // 2. Add the entity to the repository
        await advertRepository.AddAsync(advert, ct);

        // 3. Persist changes to the database
        await unitOfWork.SaveChangesAsync(ct);

        // 4. Return the generated Guid as a successful Result
        return Result.Success(advert.Guid);
    }
}