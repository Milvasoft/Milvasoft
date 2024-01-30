using System.Runtime.Serialization;

namespace Milvasoft.Components.Rest.Response;

/// <summary>
/// Listed or paginated response.
/// </summary>
/// <typeparam name="T"></typeparam>
[DataContract]
public class ListResponse<T> : Response<List<T>>
{
    /// <summary>
    /// Current page number.
    /// </summary>
    [DataMember]
    public int? CurrentPageNumber { get; set; }

    /// <summary>
    /// Total page count.
    /// </summary>
    [DataMember]
    public int? TotalPageCount { get; set; }

    /// <summary>
    /// Total data count.
    /// </summary>
    [DataMember]
    public int? TotalDataCount { get; set; }

    private ListResponse() : base()
    {
    }

    public ListResponse(bool isSuccess,
                        string message,
                        List<T> data,
                        int? currentPage,
                        int? totalPage,
                        int? totalData) : this(isSuccess,
                                               new List<ResponseMessage>() { new() { Message = message } },
                                               data,
                                               currentPage,
                                               totalPage,
                                               totalData)
    {
    }

    public ListResponse(bool isSuccess,
                        List<ResponseMessage> messages,
                        List<T> data,
                        int? currentPage,
                        int? totalPage,
                        int? totalData) : base(data)
    {
        IsSuccess = isSuccess;
        Messages = messages;
        CurrentPageNumber = currentPage;
        TotalPageCount = totalPage;
        TotalDataCount = totalData;
    }

}
