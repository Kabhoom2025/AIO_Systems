import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  emoji: string;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Point of Sale',
    emoji: '🧾',
    description: (
      <>
        Take dine-in, takeaway, and delivery orders fast, with add-ons,
        discounts, and loyalty points built in.
      </>
    ),
  },
  {
    title: 'Live Kitchen Display',
    emoji: '👨‍🍳',
    description: (
      <>
        Orders reach the kitchen the instant they're placed — no paper
        tickets, no missed items.
      </>
    ),
  },
  {
    title: 'Direct Printing',
    emoji: '🖨️',
    description: (
      <>
        Connect a USB, network, or Zebra printer once and print receipts and
        labels with zero preview dialogs.
      </>
    ),
  },
];

function Feature({title, emoji, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <span className={styles.featureEmoji} role="img" aria-label={title}>{emoji}</span>
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
