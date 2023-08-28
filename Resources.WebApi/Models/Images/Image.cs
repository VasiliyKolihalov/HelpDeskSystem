using System.ComponentModel.DataAnnotations.Schema;

namespace Resources.WebApi.Models.Images;

public class Image
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public byte[] Content { get; set; }
}