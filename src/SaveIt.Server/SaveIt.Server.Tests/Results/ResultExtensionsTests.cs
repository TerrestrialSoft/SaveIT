using SaveIt.Server.UI.Results;
using SaveIt.Server.UI.Results.Errors;
using System.Net;

namespace SaveIt.Server.Tests.Results;
public class ResultExtensionsTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK, true, null)]
    [InlineData(HttpStatusCode.NoContent, true, null)]
    [InlineData(HttpStatusCode.BadRequest, false, typeof(ApiErrors.GeneralError))]
    [InlineData(HttpStatusCode.Unauthorized, false, typeof(ApiErrors.UnAuthorizedError))]
    [InlineData(HttpStatusCode.Forbidden, false, typeof(ApiErrors.ForbiddenError))]
    [InlineData(HttpStatusCode.NotFound, false, typeof(ApiErrors.NotFoundError))]
    [InlineData(HttpStatusCode.InternalServerError, false, typeof(ApiErrors.InternalServerError))]
    [InlineData(HttpStatusCode.NotImplemented, false, null)]
    public void ToResult_ShouldReturnCorrectResult(HttpStatusCode inputCode, bool isSuccessExpected, Type? expectedErrorType)
    {
        // Arrange
        var response = new HttpResponseMessage(inputCode);

        // Act
        var result = response.ToFluentResult();

        // Assert
        Assert.True(result.IsSuccess == isSuccessExpected);

        if (expectedErrorType is not null)
        {
            Assert.Equal(expectedErrorType, result.Errors[0].GetType());
        }
    }
}
