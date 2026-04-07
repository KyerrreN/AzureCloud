import { apiClient } from "./apiClient";

export interface ApiResult<T> {
  value: T;
  isSuccess: boolean;
  error: string | null;
}

export const authApi = {
  exchangeSpotifyCode: async (code: string): Promise<ApiResult<boolean>> => {
    const response = await apiClient.post<ApiResult<boolean>>(
      "/settings/spotify/saveToken",
      { code },
    );
    return response.data;
  },
};
