import * as React from "react";
import { graphql } from "gatsby";

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
        <h2>ダウンロード(MT4用)</h2>
        <ul>
          <li>
            <a href="/downloads/TrsysPublisher.ex4">配信用</a>
          </li>
          <li>
            <a href="/downloads/TrsysSubscriber.ex4">受信用</a>
          </li>
        </ul>
      </div>
      <div>
        <h2>ダウンロード(MT5用)</h2>
        <ul>
          <li>
            <a href="/downloads/TrsysPublisher.ex5">配信用</a>
          </li>
          <li>
            <a href="/downloads/TrsysSubscriber.ex5">受信用</a>
          </li>
        </ul>
      </div>
      <div>
        <h2>設定</h2>
        <div>
          <img
            src="/images/mt4-option-ea-red.png"
            alt="MT4 の EA のオプションの設定"
          />
        </div>
        <ol>
          <li>メニューのツール -&gt; オプションをクリックする</li>
          <li>自動売買を許可するにチェックを入れる</li>
          <li>WebRequest を許可するURLリストにチェックを入れる</li>
          <li>
            +新しい URL を追加をダブルクリックし、
            <code>https://kopi-trading-system.azurewebsites.net</code>{" "}
            を入力する
          </li>
        </ol>
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
