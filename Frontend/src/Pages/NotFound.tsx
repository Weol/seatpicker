import * as React from "react";
import Typography from "@mui/material/Typography";
import { Stack } from "@mui/material";

export default function NotFound() {
  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      <Typography variant="h1" component="h1" gutterBottom>
        404
      </Typography>
      <Typography variant="body1" component="h1" align="center" gutterBottom>
        Could not find whatever you were looking for
      </Typography>
    </Stack>
  );
}
