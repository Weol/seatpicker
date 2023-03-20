import config from "../config"

export default function RedirectToDiscordLogin() {
  window.location.replace(config.DiscordAuthorizationUrl)
}