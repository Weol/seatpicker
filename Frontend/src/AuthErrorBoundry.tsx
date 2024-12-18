import { Component } from "react"
import { useAuth } from "./Adapters/AuthAdapter"
import {useNavigate} from "react-router-dom";
import ErrorPage from "./Pages/ErrorPage";

type AuthErrorBoundryProps = {
  children: React.ReactElement
}

export default function AuthErrorBoundry(props: AuthErrorBoundryProps) {
  const { logout } = useAuth();
  const navigate = useNavigate();
  
  return <ErrorBoundary logout={logout} navigate={navigate} >{props.children}</ErrorBoundary>
}

type ErrorBoundryProps = AuthErrorBoundryProps & {
  logout: () => void
  navigate: (path: string) => void
}

class ErrorBoundary extends Component<ErrorBoundryProps, { error: boolean }> {
  constructor(props: ErrorBoundryProps) {
    super(props)
    this.state = { error: false }
  }
  
  componentDidCatch(error: Error & { status?: number }) {
    if (error.status == 401) {
      this.setState({ error: true });
      
      setTimeout(() => {
        this.props.logout()
        setTimeout(() => {
          this.props.navigate("/")
          window.location.reload();
        }, 2000)
      },0)
    }
  }

  render() {
    if (this.state.error) return (<ErrorPage header={"Unauthorized"} message={"Logging you out"} />)
    return this.props.children
  }
}
