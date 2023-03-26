type Config = {
  ApiBaseUrl: string,
  DiscordAuthorizationUrl: string,
  DiscordAvatarBaseUrl: string
}

let config: Config = {
  ApiBaseUrl: "api",
  DiscordAuthorizationUrl: "https://discord.com/api/oauth2/authorize?client_id=1042448593312821248&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fredirect-login&response_type=code&scope=identify",
  DiscordAvatarBaseUrl: "https://cdn.discordapp.com/avatars/"
}

export default config;