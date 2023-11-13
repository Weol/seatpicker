export default function RedirectToDiscordLogin() {
  const redirectUrl = window.location.origin + "/redirect-login";
  const uri = `https://discord.com/api/oauth2/authorize?client_id=1042448593312821248&redirect_uri=${encodeURIComponent(
    redirectUrl
  )}&response_type=code&scope=identify`;
  window.location.replace(uri);
}
