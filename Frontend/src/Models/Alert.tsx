export default interface Alert {
  title: string;
  description?: string;
  type: "info" | "success" | "warning" | "error" | "loading",
}
