"use client";

import { useEffect, useRef, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { authApi } from "@/services/authApiClient";

const SpotifyCallbackContent = () => {
  const searchParams = useSearchParams();
  const router = useRouter();
  const called = useRef(false);

  useEffect(() => {
    const code = searchParams.get("code");

    if (code && !called.current) {
      called.current = true;

      authApi
        .exchangeSpotifyCode(code)
        .then((result) => {
          if (result.isSuccess) {
            router.push("/");
          } else {
            console.error(
              "Error while exchanging spotify code: " + result.error,
            );
            router.push("/");
          }
        })
        .catch(() => router.push("/"));
    }
  }, [searchParams, router]);

  return (
    <div
      style={{ display: "flex", justifyContent: "center", marginTop: "50px" }}
    >
      <h2>Авторизация в Spotify... Пожалуйста, подождите.</h2>
    </div>
  );
};

export default function SpotifyCallback() {
  return (
    <Suspense
      fallback={
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            marginTop: "50px",
          }}
        >
          <h2>Авторизация в Spotify... Пожалуйста, подождите.</h2>
        </div>
      }
    >
      <SpotifyCallbackContent />
    </Suspense>
  );
}
