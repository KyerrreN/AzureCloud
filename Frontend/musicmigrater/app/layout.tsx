import { Header } from "@/components/common/Header";
import { Providers } from "./Providers";

export const metadata = {
  title: "MusicMigrater",
  description: "Перенос музыки из VK в Spotify",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ru">
      <body>
        <Providers>
          <Header />

          <main>{children}</main>
        </Providers>
      </body>
    </html>
  );
}
