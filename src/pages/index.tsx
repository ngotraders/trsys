import * as React from "react";
import { graphql, Link } from "gatsby";

// Please note that you can use https://github.com/dotansimha/graphql-code-generator
// to generate all types from graphQL schema
interface IndexPageProps {
  data: {
    site: {
      siteMetadata: {
        title: string;
      };
    };
  };
}

const IndexPage = (props: IndexPageProps) => {
  return (
    <div>
      <h1>コピートレードツール</h1>
      <div>
        <h2>ダウンロード</h2>
        <ul>
          <li>
            <a href="/downloads/TrsysPublisher.ex4">配信用</a>
          </li>
          <li>
            <a href="/downloads/TrsysSubscriber.ex4">受信用</a>
          </li>
        </ul>
      </div>
    </div>
  );
};

export default IndexPage;

export const pageQuery = graphql`
  query IndexQuery {
    site {
      siteMetadata {
        title
      }
    }
  }
`;
