import { Stack } from "@mui/material"
import Typography from "@mui/material/Typography"

export default function ErrorPage(props: { header?: string; message?: string }) {
  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      <Typography variant="h1" component="h1" gutterBottom>
        {props.header ?? "Noe gikk galt ☠️"}
      </Typography>
      {props.message && (
        <Typography variant="body1" component="h1" align="center" gutterBottom>
          {props.message}
        </Typography>
      )}
    </Stack>
  )
}
