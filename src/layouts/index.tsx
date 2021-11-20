import * as React from "react";
import { Link } from "gatsby";
import Helmet from "react-helmet";

import "./index.css";

const Header = () => (
  <div
    style={{
      background: "rebeccapurple",
      marginBottom: "1.45rem",
    }}
  >
    <div
      style={{
        margin: "0 auto",
        maxWidth: 960,
        padding: "1.45rem 1.0875rem",
      }}
    >
      <h1 style={{ margin: 0 }}>
        <Link
          to="/"
          style={{
            color: "white",
            textDecoration: "none",
          }}
        >
          Trsys
        </Link>
      </h1>
    </div>
    <Devenv />
  </div>
);

const Devenv = () => (
  <div
    style={{
      background: "green",
    }}
  >
    <div
      style={{
        margin: "0 auto",
        maxWidth: 960,
        padding: "0.5rem 1.0875rem",
      }}
    >
      <h3 style={{ margin: 0, color: "white" }}>検証環境</h3>
    </div>
  </div>
);

interface DefaultLayoutProps extends React.HTMLProps<HTMLDivElement> {
  location: {
    pathname: string;
  };
  children: any;
}

const DefaultLayout = (props: DefaultLayoutProps) => {
  return (
    <div>
      <Helmet
        title="Trsys"
        meta={[
          { name: "description", content: "コピートレード" },
          { name: "keywords", content: "MT4,EA,コピートレード" },
        ]}
      />
      <Header />
      <div
        style={{
          margin: "0 auto",
          maxWidth: 960,
          padding: "0px 1.0875rem 1.45rem",
          paddingTop: 0,
        }}
      >
        {props.children}
      </div>
    </div>
  );
};

export default DefaultLayout;
