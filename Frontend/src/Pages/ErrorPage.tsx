import { Stack } from "@mui/material"
import Typography from "@mui/material/Typography"

export default function ErrorPage(props: { header?: string; subtitle?: string }) {
  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      {props.header && (
        <Typography variant="h1" component="h1" gutterBottom>
          {props.header}
        </Typography>
      )}
      {(props.subtitle ?? !props.header) && (
        <Typography variant="body1" component="h1" align="center" gutterBottom>
          {props.subtitle ?? "Noe gikk galt"}
        </Typography>
      )}
    </Stack>
  )
}
