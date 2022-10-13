using System.Security.Cryptography;

namespace SaveIT.Core.Services;
public static class RandomGenerator
{
	public static string GetRandomlyGenerateBase64String(int size)
		=> Convert.ToBase64String(RandomNumberGenerator.GetBytes(size));
}
