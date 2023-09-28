using Microsoft.EntityFrameworkCore;

namespace Resources.Api.Models.Images;

[PrimaryKey(nameof(MessageId), nameof(ImageId))]
public class ImageMessage
{
    public Guid MessageId { get; set; }
    public Guid ImageId { get; set; }

    public Image Image { get; set; }
}