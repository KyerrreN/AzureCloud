import { VkTokenData } from "@/dto/vkTokenDto";

export const extractVkTokenFromUrl = (url: string): VkTokenData | null => {
  try {
    const urlObj = new URL(url);

    const hashString = urlObj.hash.substring(1);
    const params = new URLSearchParams(hashString);

    const token = params.get("access_token");
    const expiresInStr = params.get("expires_in");

    if (!token) {
      return null;
    }

    const expiresIn = expiresInStr ? parseInt(expiresInStr, 10) : 0;

    return { token, expiresIn };
  } catch (error) {
    console.error("Не удалось распарсить URL ВКонтакте:", error);
    return null;
  }
};
