export const getSpotifyAuthUrl = () => {
  const rootUrl = "https://accounts.spotify.com/authorize";

  const params = new URLSearchParams({
    client_id: process.env.NEXT_PUBLIC_SPOTIFY_CLIENT_ID || "",
    response_type: "code",
    redirect_uri: process.env.NEXT_PUBLIC_SPOTIFY_REDIRECT_URI || "",
    scope: process.env.NEXT_PUBLIC_SPOTIFY_SCOPES || "",
    show_dialog: "true",
  });

  return `${rootUrl}?${params.toString()}`;
};
