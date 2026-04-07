import axios, { type AxiosRequestConfig, AxiosError } from "axios";

export interface AppResult<T = void> {
  value: T | null;
  isSuccess: boolean;
  error: string;
}

export const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error: AxiosError) => {
    if (
      error.response?.data &&
      typeof error.response.data === "object" &&
      "isSuccess" in error.response.data
    ) {
      return Promise.resolve(error.response);
    }

    const fallbackResult: AppResult<null> = {
      value: null,
      isSuccess: false,
      error: error.message || "Неизвестная ошибка сети",
    };

    return Promise.resolve({ data: fallbackResult });
  },
);

export const api = {
  get: async <T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AppResult<T>> => {
    const response = await apiClient.get<AppResult<T>>(url, config);
    return response.data;
  },

  post: async <T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig,
  ): Promise<AppResult<T>> => {
    const response = await apiClient.post<AppResult<T>>(url, data, config);
    return response.data;
  },

  put: async <T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig,
  ): Promise<AppResult<T>> => {
    const response = await apiClient.put<AppResult<T>>(url, data, config);
    return response.data;
  },

  delete: async <T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AppResult<T>> => {
    const response = await apiClient.delete<AppResult<T>>(url, config);
    return response.data;
  },
};
